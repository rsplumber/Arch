namespace Core.ServiceConfigs.Events;

public sealed record ServiceConfigCreatedEvent(Guid Id) : DomainEvent
{
    public override string Name => "arch.service-config.created";
}