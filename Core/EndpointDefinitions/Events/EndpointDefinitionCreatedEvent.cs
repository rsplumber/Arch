namespace Core.EndpointDefinitions.Events;

public sealed record EndpointDefinitionCreatedEvent(Guid Id , Guid ServiceConfigId) : DomainEvent
{
    public override string Name => "arch_endpoint-definition_created";
}