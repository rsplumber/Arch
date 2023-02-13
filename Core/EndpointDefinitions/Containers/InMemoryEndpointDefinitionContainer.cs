using System.Collections.Concurrent;

namespace Core.EndpointDefinitions.Containers;

public sealed class InMemoryEndpointDefinitionContainer : IEndpointDefinitionContainer
{
    private static readonly ConcurrentDictionary<DefinitionKey, ContainerEndpointDefinition> EndpointDefinitions = new();

    public ValueTask<DefinitionKey> AddAsync(EndpointDefinition endpointDefinition, CancellationToken cancellationToken = default)
    {
        var key = DefinitionKey.From(endpointDefinition.Pattern, endpointDefinition.Method);
        var containerEndpoint = new ContainerEndpointDefinition(
            endpointDefinition.ServiceConfig.Name,
            endpointDefinition.Pattern,
            endpointDefinition.Endpoint,
            endpointDefinition.Method,
            endpointDefinition.Meta.ToDictionary(a => a.Key, a => string.Join(";", a.Value!))
        );
        EndpointDefinitions.TryAdd(key, containerEndpoint);
        return ValueTask.FromResult(key);
    }

    public ValueTask<ContainerEndpointDefinition?> GetAsync(DefinitionKey key, CancellationToken cancellationToken = default)
    {
        EndpointDefinitions.TryGetValue(key, out var value);
        return ValueTask.FromResult(value);
    }

    public ContainerEndpointDefinition? Get(DefinitionKey key)
    {
        EndpointDefinitions.TryGetValue(key, out var definition);
        return definition;
    }

    public void Clear()
    {
        EndpointDefinitions.Clear();
    }

    public ValueTask RemoveAsync(DefinitionKey key, CancellationToken cancellationToken = default)
    {
        EndpointDefinitions.TryRemove(key, out _);
        return ValueTask.CompletedTask;
    }
}