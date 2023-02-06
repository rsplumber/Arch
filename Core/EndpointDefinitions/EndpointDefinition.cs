using Core.Metas;
using Core.ServiceConfigs;

namespace Core.EndpointDefinitions;

public sealed class EndpointDefinition
{
    public Guid Id { get; set; }

    public string Pattern { get; set; } = default!;

    public string Endpoint { get; set; } = default!;

    public string Method { get; set; } = default!;

    public List<Meta> Meta { get; set; } = new();

    public ServiceConfig ServiceConfig { get; set; } = default!;
}