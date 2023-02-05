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
        dynamic archEndpointDefinition = context.Items[ArchEndpointDefinitionKey]!;
        dynamic? allowAnonymous = null;
        foreach (var meta in archEndpointDefinition.Meta)
        {
            if (meta.Key != AllowAnonymousMetaKey) continue;
            allowAnonymous = meta;
            break;
        }

        if (allowAnonymous is not null)
        {
            await next(context);
            return;
        }

        var serviceSecret = ExtractServiceSecret();
        if (serviceSecret is null)
        {
            throw new UnAuthorizedException();
        }

        dynamic? info = context.Items[RequestInfoKey];
        string? tokenValue;
        try
        {
            tokenValue = info.Headers["Authorization"];
        }
        catch (Exception)
        {
            throw new UnAuthorizedException();
        }

        if (tokenValue is null)
        {
            throw new UnAuthorizedException();
        }

        var allowedPermissions = AllowedPermissions();
        string? userId = null;
        if (allowedPermissions is not null)
        {
            userId = await KunderaAuthorizePermissionAsync(tokenValue, serviceSecret, allowedPermissions);
        }
        else
        {
            var allowedRoles = AllowedRoles();
            if (allowedRoles is not null)
            {
                userId = await KunderaAuthorizeRoleAsync(tokenValue, serviceSecret, allowedRoles);
            }
        }

        if (userId is null)
        {
            throw new UnAuthorizedException();
        }

        context.Items[UserIdKey] = userId;

        await next(context);

        string? ExtractServiceSecret()
        {
            foreach (var meta in archEndpointDefinition.Meta)
            {
                if (meta.Key != ServiceSecretMetaKey) continue;
                return (string) meta.Value;
            }

            return null;
        }

        string[]? AllowedPermissions()
        {
            foreach (var meta in archEndpointDefinition.Meta)
            {
                if (meta.Key != PermissionsMetaKey) continue;
                return ((string) meta.Value).Split(",");
            }

            return null;
        }

        string[]? AllowedRoles()
        {
            foreach (var meta in archEndpointDefinition.Meta)
            {
                if (meta.Key != RolesMetaKey) continue;
                return ((string) meta.Value).Split(",");
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