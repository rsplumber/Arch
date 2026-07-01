namespace Arch.Core.ServiceConfigs.EndpointDefinitions.Events;

/// <summary>
/// Raised when a new endpoint is added to a service. Carries the routing data (pattern, raw
/// endpoint, method) so subscribers can update the in-memory endpoint tree and definition cache
/// without depending on the entity's database-assigned identity being available yet.
/// </summary>
public sealed record EndpointDefinitionCreatedEvent(Guid Id, Guid ServiceConfigId, string Pattern, string Endpoint, string Method) : DomainEvent
{
    public override string Name => "arch.endpoint-definition.created";
}
