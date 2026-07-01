using Arch.Core.ServiceConfigs;
using Arch.Core.ServiceConfigs.EndpointDefinitions;

namespace Arch.Core.Pipeline.Models;

public sealed record ResolvedEndpoint
{
    private const string LoggingMetaKey = "logging";

    public required Guid Id { get; init; }

    public required string Endpoint { get; init; }

    public required HttpMethod Method { get; init; }

    public required IReadOnlyDictionary<string, string> Meta { get; init; }

    public required ResolvedService Service { get; init; }

    public LoggingOptions Logging => new(Meta.GetValueOrDefault(LoggingMetaKey));

    public static ResolvedEndpoint From(EndpointDefinition definition) => new()
    {
        Id = definition.Id,
        Endpoint = definition.Endpoint,
        Method = definition.Method,
        Meta = definition.Meta,
        Service = ResolvedService.From(definition.ServiceConfig)
    };
}

public sealed record ResolvedService
{
    private const string IgnoreDispatchKey = "ignore_dispatch";

    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required IReadOnlyList<string> BaseUrls { get; init; }

    public required IReadOnlyDictionary<string, string> Meta { get; init; }

    public bool IgnoreDispatch() => Meta.ContainsKey(IgnoreDispatchKey);

    public static ResolvedService From(ServiceConfig serviceConfig) => new()
    {
        Id = serviceConfig.Id,
        Name = serviceConfig.Name,
        BaseUrls = serviceConfig.BaseUrls,
        Meta = serviceConfig.Meta
    };
}
