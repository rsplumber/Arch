using System.Text;
using System.Text.Json;
using Arch.Kundera.Exceptions;
using Core.Library;
using Microsoft.AspNetCore.Http;

namespace Arch.Kundera;

internal sealed class KunderaAuthorizationMiddleware : ArchMiddleware
{
    private readonly IHttpClientFactory _clientFactory;
    private const string HttpClientFactoryKey = "kundera";
    private const string ServiceSecretMetaKey = "service_secret";
    private const string AllowAnonymousMetaKey = "allow_anonymous";
    private const string PermissionsMetaKey = "permissions";
    private const string RolesMetaKey = "roles";
    private static readonly string AuthorizePermissionUrl = $"{KunderaAuthorizationSettings.BaseUrl}/authorize/permission";
    private static readonly string AuthorizeRoleUrl = $"{KunderaAuthorizationSettings.BaseUrl}/authorize/role";
    private const int ForbiddenCode = 403;
    private const int UnAuthorizedCode = 401;
    private const int SessionExpiredCode = 440;


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
            tokenValue = RequestInfo!.Headers?["Authorization"];
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
    }

    private async Task<(int, string?)> KunderaAuthorizePermissionAsync(string token, string serviceSecret, IEnumerable<string> permissions)
    {
        var client = _clientFactory.CreateClient(HttpClientFactoryKey);
        var result = await client.PostAsync(AuthorizePermissionUrl, new StringContent(JsonSerializer.Serialize(new
        {
            Authorization = token,
            serviceSecret,
            Actions = permissions
        }), Encoding.UTF8, "application/json"));
        var response = await result.Content.ReadAsStringAsync();
        var userId = result.IsSuccessStatusCode ? response.Replace("\"", "") : null;
        return ((int) result.StatusCode, userId);
    }

    private async Task<(int, string?)> KunderaAuthorizeRoleAsync(string token, string serviceSecret, IEnumerable<string> roles)
    {
        var client = _clientFactory.CreateClient(HttpClientFactoryKey);
        var result = await client.PostAsync(AuthorizeRoleUrl, new StringContent(JsonSerializer.Serialize(new
        {
            Authorization = token,
            serviceSecret,
            Roles = roles
        }), Encoding.UTF8, "application/json"));
        var response = await result.Content.ReadAsStringAsync();
        var userId = result.IsSuccessStatusCode ? response.Replace("\"", "") : null;
        return ((int) result.StatusCode, userId);
    }
}