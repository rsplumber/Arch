using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Arch.Kundera.Exceptions;
using Core;
using KunderaNet.Services.Authorization.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Arch.Kundera;

internal sealed class KunderaAuthorizationMiddleware : ArchMiddleware
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
    private readonly IAuthorizeService _authorizeService;

    public KunderaAuthorizationMiddleware(IAuthorizeService authorizeService)
    {
        _authorizeService = authorizeService;
    }


    public override async Task HandleAsync(HttpContext context, RequestDelegate next)
    {
        if (AllowAnonymous())
        {
            await next(context);
            return;
        }

        string? tokenValue;
        try
        {
            tokenValue = RequestInfo!.Headers[AuthorizationHeaderKey];
        }
        catch (Exception)
        {
            throw new KunderaUnAuthorizedException();
        }

        if (tokenValue is null)
        {
            throw new KunderaUnAuthorizedException();
        }

        var (allowedPermissions, allowedRoles) = AllowedPermissionsAndRoles();
        if (allowedPermissions is null && allowedRoles is null)
        {
            throw new KunderaUnAuthorizedException();
        }

        var serviceSecret = ExtractServiceSecret();

        AuthorizedResponse? authorized;
        int code;
        var headers = GenerateHeaders();
        if (allowedPermissions is not null && allowedRoles is null)
        {
            (code, authorized) = await _authorizeService.AuthorizePermissionAsync(tokenValue, allowedPermissions, headers);
        }
        else if (allowedRoles is not null && allowedPermissions is null)
        {
            (code, authorized) = await _authorizeService.AuthorizeRoleAsync(tokenValue, allowedRoles, headers);
        }
        else
        {
            throw new KunderaMultipleAuthorizationTypeException();
        }

        switch (code)
        {
            case ForbiddenCode:
                throw new KunderaForbiddenException();
            case UnAuthorizedCode:
                throw new KunderaUnAuthorizedException();
            case SessionExpiredCode:
                throw new KunderaSessionExpiredException();
        }

        if (authorized is null)
        {
            throw new KunderaUnAuthorizedException();
        }

        context.Items[UserIdKey] = authorized.UserId;
        context.Items[UserTokenKey] = GenerateUserToken(serviceSecret);

        await next(context);

        bool AllowAnonymous() => GetMeta(AllowAnonymousMetaKey) is not null;

        (string[]?, string[]?) AllowedPermissionsAndRoles()
        {
            var permissionMeta = GetMeta(PermissionsMetaKey);
            var roleMeta = GetMeta(RolesMetaKey);
            return (permissionMeta?.Split(","), roleMeta?.Split(","));
        }

        string ExtractServiceSecret()
        {
            var secret = GetMeta(ServiceSecretMetaKey);
            if (secret is null)
            {
                throw new KunderaServiceSecretNotDefinedException();
            }

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