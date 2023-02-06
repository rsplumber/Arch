using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Arch.Kundera;

internal class KunderaAuthorizationMiddleware : IMiddleware
{
    private const string HttpClientFactoryKey = "kundera";
    private readonly IHttpClientFactory _clientFactory;
    private const string ServiceSecretMetaKey = "service_secret";
    private const string AllowAnonymousMetaKey = "allow_anonymous";
    private const string PermissionsMetaKey = "permissions";
    private const string RolesMetaKey = "roles";
    private const string UserIdKey = "user_id";
    private const string ArchEndpointDefinitionKey = "arch_endpoint_definition";
    private const string RequestInfoKey = "request_info";
    private static readonly string AuthorizePermissionUrl = $"{KunderaAuthorizationSettings.BaseUrl}/authorize/permission";
    private static readonly string AuthorizeRoleUrl = $"{KunderaAuthorizationSettings.BaseUrl}/authorize/role";


    public KunderaAuthorizationMiddleware(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        dynamic endpointDefinition = context.Items[ArchEndpointDefinitionKey]!;

        if (AllowAnonymous())
        {
            await next(context);
            return;
        }

        dynamic? requestInfo = context.Items[RequestInfoKey];
        string? tokenValue;
        try
        {
            tokenValue = requestInfo.Headers["Authorization"];
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
        if (serviceSecret is null)
        {
            throw new KunderaServiceSecretNotDefinedException();
        }

        string? userId;
        if (allowedPermissions is not null && allowedRoles is null)
        {
            userId = await KunderaAuthorizePermissionAsync(tokenValue, serviceSecret, allowedPermissions);
        }
        else if (allowedRoles is not null && allowedPermissions is null)
        {
            userId = await KunderaAuthorizeRoleAsync(tokenValue, serviceSecret, allowedRoles);
        }
        else
        {
            throw new KunderaMultipleAuthorizationTypeException();
        }

        if (userId is null)
        {
            throw new KunderaUnAuthorizedException();
        }

        context.Items[UserIdKey] = userId;

        await next(context);

        bool AllowAnonymous()
        {
            foreach (var meta in endpointDefinition.Meta)
            {
                if (meta.Key != AllowAnonymousMetaKey) continue;
                return true;
            }

            return false;
        }

        (string[]?, string[]?) AllowedPermissionsAndRoles()
        {
            string[]? permissions = null;
            string[]? roles = null;
            foreach (var meta in endpointDefinition.Meta)
            {
                if (meta.Key == PermissionsMetaKey)
                {
                    permissions = ((string) meta.Value).Split(",");
                }
                else if (meta.Key == RolesMetaKey)
                {
                    roles = ((string) meta.Value).Split(",");
                }
            }

            return (permissions, roles);
        }

        string? ExtractServiceSecret()
        {
            foreach (var meta in endpointDefinition.Meta)
            {
                if (meta.Key != ServiceSecretMetaKey) continue;
                return (string) meta.Value;
            }

            return null;
        }
    }

    private async Task<string?> KunderaAuthorizePermissionAsync(string token, string serviceSecret, IEnumerable<string> permissions)
    {
        var client = _clientFactory.CreateClient(HttpClientFactoryKey);
        var result = await client.PostAsync(AuthorizePermissionUrl, new StringContent(JsonSerializer.Serialize(new
        {
            Authorization = token,
            serviceSecret,
            Actions = permissions
        }), Encoding.UTF8, "application/json"));
        var response = await result.Content.ReadAsStringAsync();
        return result.IsSuccessStatusCode ? response.Replace("\"", "") : null;
    }

    private async Task<string?> KunderaAuthorizeRoleAsync(string token, string serviceSecret, IEnumerable<string> roles)
    {
        var client = _clientFactory.CreateClient(HttpClientFactoryKey);
        var result = await client.PostAsync(AuthorizeRoleUrl, new StringContent(JsonSerializer.Serialize(new
        {
            Authorization = token,
            serviceSecret,
            Roles = roles
        }), Encoding.UTF8, "application/json"));
        var response = await result.Content.ReadAsStringAsync();
        return result.IsSuccessStatusCode ? response.Replace("\"", "") : null;
    }
}