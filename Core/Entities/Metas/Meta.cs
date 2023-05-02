using Core.Entities.EndpointDefinitions;
using Core.Entities.ServiceConfigs;

namespace Core.Entities.Metas;

public sealed class Meta : BaseEntity
{
    public Guid Id { get; set; }

    public string Key { get; set; } = default!;

    public string Value { get; set; } = default!;

    public EndpointDefinition? EndpointDefinition { get; set; }

    public ServiceConfig? ServiceConfig { get; set; }
}