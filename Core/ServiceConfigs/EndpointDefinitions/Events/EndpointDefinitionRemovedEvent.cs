namespace Arch.Core.ServiceConfigs.EndpointDefinitions.Events;

public sealed record EndpointDefinitionRemovedEvent(Guid Id, Guid ServiceConfigId) : DomainEvent
{
    public override string Name => "arch.endpoint-definition.removed";
}