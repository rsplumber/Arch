namespace Arch.Core.ServiceConfigs.Events;

/// <summary>
/// A snapshot of the routing data of an endpoint that belonged to a removed service, captured
/// before the cascade delete so subscribers can evict it from the in-memory tree and cache.
/// </summary>
public sealed record RemovedEndpoint(string Pattern, string Endpoint, string Method);
