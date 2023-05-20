using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Arch.Kundera.Exceptions;
using Core;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Arch.Kundera;

internal sealed class KunderaAuthorizationMiddleware : ArchMiddleware
{
    private readonly IHttpClientFactory _clientFactory;
    private const string HttpClientFactoryKey = "kundera";
    private const string ServiceSecretMetaKey = "service_secret";
    private const string AllowAnonymousMetaKey = "allow_anonymous";
    private const string PermissionsMetaKey = "permissions";
    private const string AuthorizationHeaderKey = "Authorization";
    private const string RolesMetaKey = "roles";
    private const string AuthorizePermissionUrl = "api/v1/authorize/permission";
    private const string AuthorizeRoleUrl = "api/v1/authorize/role";
    private const int ForbiddenCode = 403;
    private const int UnAuthorizedCode = 401;
    private const int SessionExpiredCode = 440;
    private static readonly JwtSecurityTokenHandler JwtSecurityTokenHandler = new();


    public KunderaAuthorizationMiddleware(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
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

        string? userId;
        int code;
        if (allowedPermissions is not null && allowedRoles is null)
        {
            (code, userId) = await KunderaAuthorizePermissionAsync(tokenValue, serviceSecret, allowedPermissions);
        }
        else if (allowedRoles is not null && allowedPermissions is null)
        {
            (code, userId) = await KunderaAuthorizeRoleAsync(tokenValue, serviceSecret, allowedRoles);
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

        if (userId is null)
        {
            throw new KunderaUnAuthorizedException();
        }

        context.Items[UserIdKey] = userId;
        context.Items[UserIdTokenKey] = GenerateUserToken(serviceSecret);

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
                Subject = new ClaimsIdentity(new[] { new Claim("id", userId) }),
                Expires = DateTime.UtcNow.AddSeconds(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)), SecurityAlgorithms.HmacSha256Signature)
            }));
    }

    private async Task<(int, string?)> KunderaAuthorizePermissionAsync(string token, string serviceSecret, IEnumerable<string> permissions)
    {
        var client = _clientFactory.CreateClient(HttpClientFactoryKey);
        client.DefaultRequestHeaders.Add("service_secret", serviceSecret);
        var result = await client.PostAsJsonAsync(AuthorizePermissionUrl, new
        {
            Authorization = token,
            Actions = permissions
        });
        var response = await result.Content.ReadAsStringAsync();
        var userId = result.IsSuccessStatusCode ? response.Replace("\"", string.Empty) : null;
        return ((int)result.StatusCode, userId);
    }

    private async Task<(int, string?)> KunderaAuthorizeRoleAsync(string token, string serviceSecret, IEnumerable<string> roles)
    {
        var client = _clientFactory.CreateClient(HttpClientFactoryKey);
        client.DefaultRequestHeaders.Add("service_secret", serviceSecret);
        var result = await client.PostAsJsonAsync(AuthorizeRoleUrl, new
        {
            Authorization = token,
            Roles = roles
        });
        var response = await result.Content.ReadAsStringAsync();
        var userId = result.IsSuccessStatusCode ? response.Replace("\"", string.Empty) : null;
        return ((int)result.StatusCode, userId);
    }
}