using Core.EndpointDefinitions;
using Core.EndpointDefinitions.Events;
using Core.Metas;

namespace Core.ServiceConfigs;

public sealed class ServiceConfig : BaseEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public bool Primary { get; set; } = default!;

    public string BaseUrl { get; set; } = default!;

    public List<EndpointDefinition> EndpointDefinitions { get; set; } = new();

    public List<Meta> Meta { get; set; } = new();

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public void Add(EndpointDefinition definition)
    {
        EndpointDefinitions.Add(definition);
        AddDomainEvent(new EndpointDefinitionCreatedEvent(definition.Id, Id));
    }

    public void Remove(EndpointDefinition definition)
    {
        EndpointDefinitions.Remove(definition);
        AddDomainEvent(new EndpointDefinitionRemovedEvent(definition.Id, Id));
    }
}