using System.Collections.Concurrent;
using Arch.Core.ServiceConfigs.EndpointDefinitions;
using Arch.Data.Caching.Abstractions;

namespace Arch.Data.Caching.InMemory;

/// <summary>
/// In-memory <see cref="IEndpointDefinitionCache"/> backed by a process-wide concurrent dictionary.
/// Registered as a singleton so the resolver and the routing-state synchronizer share one store.
/// </summary>
internal sealed class InMemoryEndpointDefinitionCache : IEndpointDefinitionCache
{
    private readonly ConcurrentDictionary<DefinitionKey, EndpointDefinition> _definitions = new();

    public bool TryGet(DefinitionKey key, out EndpointDefinition? definition) => _definitions.TryGetValue(key, out definition);

    public void Set(DefinitionKey key, EndpointDefinition definition) => _definitions[key] = definition;

    public void Remove(DefinitionKey key) => _definitions.TryRemove(key, out _);
}
