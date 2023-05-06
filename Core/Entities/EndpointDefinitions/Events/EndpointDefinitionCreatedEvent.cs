namespace Core.Entities.EndpointDefinitions.Events;

public sealed record EndpointDefinitionCreatedEvent(Guid Id, Guid ServiceConfigId) : DomainEvent
{
    public override string Name => "arch.endpoint-definition.created";
}