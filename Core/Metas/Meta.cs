using Core.EndpointDefinitions;
using Core.ServiceConfigs;

namespace Core.Metas;

public sealed class Meta : BaseEntity
{
    public Guid Id { get; set; }

    public string Key { get; set; } = default!;

    public string Value { get; set; } = default!;

    public EndpointDefinition? EndpointDefinition { get; set; }

    public ServiceConfig? ServiceConfig { get; set; }
}