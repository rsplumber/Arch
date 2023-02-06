using Core.EndpointDefinitions;
using Core.Metas;

namespace Core.ServiceConfigs;

public sealed class ServiceConfig
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public List<EndpointDefinition> EndpointDefinitions { get; set; } = new();

    public List<Meta> Meta { get; set; } = new();
}