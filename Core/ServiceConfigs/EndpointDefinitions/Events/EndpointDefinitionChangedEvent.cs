namespace Arch.Core.ServiceConfigs.EndpointDefinitions.Events;

public sealed record EndpointDefinitionChangedEvent(Guid Id) : DomainEvent
{
    public override string Name => "arch.endpoint-definition.changed";
}