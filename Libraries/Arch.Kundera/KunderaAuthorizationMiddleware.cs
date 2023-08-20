using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Extensions;
using Core.Pipeline;
using Core.Pipeline.Models;
using FastEndpoints;
using KunderaNet.Services.Authorization.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Arch.Kundera;

internal sealed class KunderaAuthorizationMiddleware : IMiddleware
{
    private const string ServiceSecretMetaKey = "service_secret";
    private const string AllowAnonymousMetaKey = "allow_anonymous";
    private const string PermissionsMetaKey = "permissions";
    private const string AuthorizationHeaderKey = "Authorization";
    private const string RolesMetaKey = "roles";
    private const int UnAuthorizedCode = 401;
    private const int ForbiddenCode = 403;
    private const int SessionExpiredCode = 440;
    private static readonly JwtSecurityTokenHandler JwtSecurityTokenHandler = new();
    private const string UserTokenKey = "user_token";
    private const string UserIdKey = "user_id";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var requestState = context.ProcessorState<RequestState>();
        if (AllowAnonymous())
        {
            await next(context);
            return;
        }

        string? tokenValue;
        try
        {
            tokenValue = context.Request.Headers()[AuthorizationHeaderKey];
        }
        catch (Exception)
        {
            await context.Response.SendUnauthorizedAsync();
            return;
        }

        var (allowedPermissions, allowedRoles) = AllowedPermissionsAndRoles();
        if (allowedPermissions is null && allowedRoles is null)
        {
            await context.Response.SendUnauthorizedAsync();
            return;
        }

        var serviceSecret = ExtractServiceSecret();
        if (serviceSecret is null)
        {
            await context.Response.SendUnauthorizedAsync();
            return;
        }

        var authorizeService = context.Resolve<IAuthorizeService>();
        AuthorizedResponse? authorized;
        int code;
        var headers = GenerateHeaders();
        if (allowedPermissions is not null && allowedRoles is null)
        {
            (code, authorized) = await authorizeService.AuthorizePermissionAsync(tokenValue, allowedPermissions, headers);
        }
        else if (allowedRoles is not null && allowedPermissions is null)
        {
            (code, authorized) = await authorizeService.AuthorizeRoleAsync(tokenValue, allowedRoles, headers);
        }
        else
        {
            await context.Response.SendStringAsync("Authorization: Multiple authorization types not supported, only use permissions or roles", 400);
            return;
        }

        switch (code)
        {
            case ForbiddenCode:
                await context.Response.SendStringAsync("Forbidden", ForbiddenCode);
                return;
            case UnAuthorizedCode:
                await context.Response.SendUnauthorizedAsync();
                return;
            case SessionExpiredCode:
                await context.Response.SendStringAsync("Session expired", SessionExpiredCode);
                return;
        }

        if (authorized is null)
        {
            await context.Response.SendUnauthorizedAsync();
            return;
        }

        requestState.Meta.Add(UserIdKey, authorized.UserId);
        requestState.RequestInfo.AttachedHeaders.Add(UserTokenKey, GenerateUserToken(serviceSecret));

        await next(context);
        return;

        bool AllowAnonymous() => requestState.EndpointDefinition.Meta.TryGetValue(AllowAnonymousMetaKey, out _);

        (string[]?, string[]?) AllowedPermissionsAndRoles()
        {
            requestState.EndpointDefinition.Meta.TryGetValue(PermissionsMetaKey, out var permission);
            requestState.EndpointDefinition.Meta.TryGetValue(RolesMetaKey, out var role);
            return (permission?.Split(","), role?.Split(","));
        }

        string? ExtractServiceSecret()
        {
            requestState.EndpointDefinition.Meta.TryGetValue(ServiceSecretMetaKey, out var secret);
            return secret;
        }

        string GenerateUserToken(string secret) => JwtSecurityTokenHandler.WriteToken(JwtSecurityTokenHandler
            .CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", authorized.UserId),
                    new Claim("service_id", authorized.ServiceId ?? string.Empty),
                    new Claim("scope_id", authorized.ScopeId ?? string.Empty),
                }),
                Expires = DateTime.UtcNow.AddSeconds(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)), SecurityAlgorithms.HmacSha256Signature)
            }));

        Dictionary<string, string> GenerateHeaders()
        {
            var requestHeaders = context.Request
                .Headers
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value[0]!);

            requestHeaders.Remove("service_secret");
            requestHeaders.Add("service_secret", serviceSecret);
            return requestHeaders;
        }
    }
}