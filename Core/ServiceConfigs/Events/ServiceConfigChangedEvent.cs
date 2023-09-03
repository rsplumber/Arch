namespace Arch.Core.ServiceConfigs.Events;

public sealed record ServiceConfigChangedEvent(Guid Id) : DomainEvent
{
    public override string Name => "arch.service-config.changed";
}