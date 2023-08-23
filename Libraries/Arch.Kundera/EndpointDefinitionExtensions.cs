using Core.EndpointDefinitions;

namespace Arch.Kundera;

internal static class EndpointDefinitionExtensions
{
    private const string PermissionsMetaKey = "permissions";
    private const string RolesMetaKey = "roles";
    private const string ServiceSecretMetaKey = "service_secret";
    private const string AllowAnonymousMetaKey = "allow_anonymous";

    public static string[] ExtractPermissions(this EndpointDefinition endpointDefinition)
    {
        var permissionMeta = endpointDefinition.Meta.FirstOrDefault(meta1 => meta1.Key == PermissionsMetaKey);
        return permissionMeta is null ? Array.Empty<string>() : permissionMeta.Value.Split(",");
    }

    public static string[] ExtractRoles(this EndpointDefinition endpointDefinition)
    {
        var rolesMeta = endpointDefinition.Meta.FirstOrDefault(meta1 => meta1.Key == RolesMetaKey);
        return rolesMeta is null ? Array.Empty<string>() : rolesMeta.Value.Split(",");
    }

    public static string ExtractServiceSecret(this EndpointDefinition endpointDefinition)
    {
        var serviceSecretMeta = endpointDefinition.Meta.FirstOrDefault(meta1 => meta1.Key == ServiceSecretMetaKey);
        return serviceSecretMeta is null ? string.Empty : serviceSecretMeta.Value;
    }

    public static bool AllowAnonymous(this EndpointDefinition endpointDefinition)
    {
        var allowAnonymousMeta = endpointDefinition.Meta.FirstOrDefault(meta1 => meta1.Key == AllowAnonymousMetaKey);
        return allowAnonymousMeta is not null;
    }
}