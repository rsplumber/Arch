namespace Core.EndpointDefinitions.Events;

public sealed record EndpointDefinitionChangedEvent(Guid Id) : DomainEvent
{
    public override string Name => "arch_endpoint-definition_changed";
}