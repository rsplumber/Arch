namespace Core.ServiceConfigs.Events;

public sealed record ServiceConfigRemovedEvent(Guid Id) : DomainEvent
{
    public override string Name => "arch.service-config.removed";
}