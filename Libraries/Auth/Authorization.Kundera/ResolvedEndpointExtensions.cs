using Arch.Core.Pipeline.Models;

namespace Arch.Authorization.Kundera;

internal static class ResolvedEndpointExtensions
{
    private const string PermissionsMetaKey = "permissions";
    private const string RolesMetaKey = "roles";
    private const string ServiceSecretMetaKey = "service_secret";
    private const string AllowAnonymousMetaKey = "allow_anonymous";

    public static string[] ExtractPermissions(this ResolvedEndpoint endpoint)
    {
        return endpoint.Meta.TryGetValue(PermissionsMetaKey, out var permissionValue) ? permissionValue.Split(",") : [];
    }

    public static string[] ExtractRoles(this ResolvedEndpoint endpoint)
    {
        return endpoint.Meta.TryGetValue(RolesMetaKey, out var rolesValue) ? rolesValue.Split(",") : [];
    }

    public static string ExtractServiceSecret(this ResolvedEndpoint endpoint)
    {
        return endpoint.Service.Meta.TryGetValue(ServiceSecretMetaKey, out var serviceSecretValue) ? serviceSecretValue : string.Empty;
    }

    public static bool AllowAnonymous(this ResolvedEndpoint endpoint)
    {
        return endpoint.Meta.ContainsKey(AllowAnonymousMetaKey);
    }
}
