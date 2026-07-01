using Arch.Core.ServiceConfigs.EndpointDefinitions;

namespace Arch.Data.Caching.Abstractions;

/// <summary>
/// Abstraction over the resolved-definition cache so the routing-state sync logic stays independent
/// of the concrete cache backend (in-memory, distributed, ...). Entries are keyed by
/// <see cref="DefinitionKey"/> (resolved pattern + method) so parametric routes share a single entry.
/// </summary>
public interface IEndpointDefinitionCache
{
    /// <summary>Reads a cached definition; returns <c>false</c> on a miss.</summary>
    bool TryGet(DefinitionKey key, out EndpointDefinition? definition);

    /// <summary>Inserts or replaces the cached definition for a key.</summary>
    void Set(DefinitionKey key, EndpointDefinition definition);

    /// <summary>Evicts a key if present.</summary>
    void Remove(DefinitionKey key);
}
