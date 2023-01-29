using System.Collections.Concurrent;

namespace Core.EndpointDefinitions.Containers;

public class InMemoryEndpointDefinitionContainer : IEndpointDefinitionContainer
{
    private static readonly ConcurrentDictionary<DefinitionKey, EndpointDefinition> EndpointDefinitions = new();

    public ValueTask AddAsync(EndpointDefinition endpointDefinition, CancellationToken cancellationToken = default)
    {
        EndpointDefinitions.TryAdd(DefinitionKey.From(endpointDefinition.Pattern, endpointDefinition.Method), endpointDefinition);
        return ValueTask.CompletedTask;
    }

    public ValueTask<EndpointDefinition?> GetAsync(DefinitionKey key, CancellationToken cancellationToken = default)
    {
        EndpointDefinitions.TryGetValue(key, out var value);
        return ValueTask.FromResult(value);
    }

    public EndpointDefinition? Get(DefinitionKey key)
    {
        EndpointDefinitions.TryGetValue(key, out var definition);
        return definition;
    }

    public ValueTask RemoveAsync(DefinitionKey key, CancellationToken cancellationToken = default)
    {
        EndpointDefinitions.TryRemove(key, out _);
        return ValueTask.CompletedTask;
    }
}