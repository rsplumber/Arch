namespace Arch.Core.ServiceConfigs.EndpointDefinitions.Events;

/// <summary>
/// Raised when an endpoint is removed from a service. Carries the routing data because by the time
/// subscribers run, the row is already gone from the database and can no longer be looked up to
/// evict it from the in-memory endpoint tree and definition cache.
/// </summary>
public sealed record EndpointDefinitionRemovedEvent(Guid Id, Guid ServiceConfigId, string Pattern, string Endpoint, string Method) : DomainEvent
{
    public override string Name => "arch.endpoint-definition.removed";
}
