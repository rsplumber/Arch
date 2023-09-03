using System.Collections.Concurrent;
using Arch.Core.EndpointDefinitions;
using Arch.EndpointGraph.Abstractions;

namespace Arch.Data.Caching.InMemory;

internal sealed class InMemoryEndpointDefinitionResolver : IEndpointDefinitionResolver
{
    private readonly IEndpointGraph _endpointPatternTree;
    private readonly IEndpointDefinitionResolver _endpointDefinitionResolver;
    private static readonly ConcurrentDictionary<DefinitionKey, EndpointDefinition> EndpointDefinitions = new();

    public InMemoryEndpointDefinitionResolver(IEndpointDefinitionResolver endpointDefinitionResolver, IEndpointGraph endpointPatternTree)
    {
        _endpointDefinitionResolver = endpointDefinitionResolver;
        _endpointPatternTree = endpointPatternTree;
    }

    public async ValueTask<(EndpointDefinition?, object[])> ResolveAsync(string url, HttpMethod method, CancellationToken cancellationToken = default)
    {
        var (pattern, pathParameters) = await _endpointPatternTree.FindAsync(url, cancellationToken);
        if (pattern is null) return Empty();

        var definitionKey = DefinitionKey.From(url, method);
        if (EndpointDefinitions.TryGetValue(definitionKey, out var cachedEndpointDefinition))
        {
            return (cachedEndpointDefinition, pathParameters);
        }

        var (endpointDefinition, _) = await _endpointDefinitionResolver.ResolveAsync(url, method, cancellationToken);
        if (endpointDefinition is null) return Empty();
        EndpointDefinitions.TryAdd(definitionKey, endpointDefinition);
        return (endpointDefinition, pathParameters);

        (EndpointDefinition?, object[]) Empty() => (null, Array.Empty<object>());
    }

    internal ConcurrentDictionary<DefinitionKey, EndpointDefinition> EndpointDefinitionsContainer => EndpointDefinitions;
}