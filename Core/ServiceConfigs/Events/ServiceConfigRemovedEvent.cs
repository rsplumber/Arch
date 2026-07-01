namespace Arch.Core.ServiceConfigs.Events;

/// <summary>
/// Raised when a service is deleted. Carries a snapshot of its endpoints' routing data so
/// subscribers can evict every cascade-deleted endpoint from the in-memory tree and cache.
/// </summary>
public sealed record ServiceConfigRemovedEvent(Guid Id, IReadOnlyList<RemovedEndpoint> Endpoints) : DomainEvent
{
    public override string Name => "arch.service-config.removed";
}
