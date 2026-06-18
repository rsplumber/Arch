using Arch.Core.ServiceConfigs.EndpointDefinitions;

namespace Arch.Authorization.SimpleJwt;

internal static class EndpointDefinitionExtensions
{
    private const string RolesMetaKey = "roles";
    private const string ServiceSecretMetaKey = "service_secret";
    private const string AllowAnonymousMetaKey = "allow_anonymous";

    public static string[] ExtractRoles(this EndpointDefinition endpointDefinition)
    {
        return endpointDefinition.Meta.TryGetValue(RolesMetaKey, out var rolesValue) ? rolesValue.Split(",") : [];
    }

    public static string ExtractServiceSecret(this EndpointDefinition endpointDefinition)
    {
        return endpointDefinition.ServiceConfig.Meta.TryGetValue(ServiceSecretMetaKey, out var serviceSecretValue) ? serviceSecretValue : string.Empty;
    }

    public static bool AllowAnonymous(this EndpointDefinition endpointDefinition)
    {
        return endpointDefinition.Meta.ContainsKey(AllowAnonymousMetaKey);
    }
}
