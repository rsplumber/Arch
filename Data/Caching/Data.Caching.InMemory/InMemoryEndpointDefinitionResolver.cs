using Arch.Core.ServiceConfigs.EndpointDefinitions;
using Arch.Data.Caching.Abstractions;
using Arch.Core.EndpointResolver;

namespace Arch.Data.Caching.InMemory;

internal sealed class InMemoryEndpointDefinitionResolver : IEndpointDefinitionResolver
{
    private readonly IEndpointGraph _endpointPatternTree;
    private readonly IEndpointDefinitionResolver _endpointDefinitionResolver;
    private readonly IEndpointDefinitionCache _cache;

    public InMemoryEndpointDefinitionResolver(IEndpointDefinitionResolver endpointDefinitionResolver, IEndpointGraph endpointPatternTree, IEndpointDefinitionCache cache)
    {
        _endpointDefinitionResolver = endpointDefinitionResolver;
        _endpointPatternTree = endpointPatternTree;
        _cache = cache;
    }

    public async ValueTask<(EndpointDefinition?, object[])> ResolveAsync(string url, HttpMethod method, CancellationToken cancellationToken = default)
    {
        var (pattern, pathParameters) = await _endpointPatternTree.FindAsync(url, cancellationToken);
        if (pattern is null) return Empty();

        // Key the cache by the resolved pattern (not the concrete url) so parametric routes share a
        // single entry and so this matches the keys the event handlers add/evict on config changes.
        var definitionKey = DefinitionKey.From(pattern, method);
        if (_cache.TryGet(definitionKey, out var cachedEndpointDefinition) && cachedEndpointDefinition is not null)
        {
            return (cachedEndpointDefinition, pathParameters);
        }

        var (endpointDefinition, _) = await _endpointDefinitionResolver.ResolveAsync(url, method, cancellationToken);
        if (endpointDefinition is null) return Empty();
        _cache.Set(definitionKey, endpointDefinition);
        return (endpointDefinition, pathParameters);

        (EndpointDefinition?, object[]) Empty() => (null, Array.Empty<object>());
    }
}
