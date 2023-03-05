namespace Core.EndpointDefinitions.Events;

public sealed record EndpointDefinitionRemovedEvent(Guid Id, Guid ServiceConfigId) : DomainEvent
{
    public override string Name => "arch_endpoint-definition_removed";
}