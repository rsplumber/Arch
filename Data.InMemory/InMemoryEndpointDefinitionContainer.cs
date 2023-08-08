using System.Collections.Concurrent;
using Core.EndpointDefinitions;

namespace Data.InMemory;

internal sealed class InMemoryEndpointDefinitionContainer : IEndpointDefinitionContainer
{
    private static readonly ConcurrentDictionary<DefinitionKey, ContainerEndpointDefinition> EndpointDefinitions = new();

    public ValueTask<DefinitionKey> AddAsync(EndpointDefinition endpointDefinition, CancellationToken cancellationToken = default)
    {
        var key = DefinitionKey.From(endpointDefinition.Pattern, endpointDefinition.Method);
        var containerEndpoint = new ContainerEndpointDefinition
        {
            BaseUrl = endpointDefinition.ServiceConfig.BaseUrl,
            Pattern = endpointDefinition.Pattern,
            Endpoint = endpointDefinition.Endpoint,
            Method = endpointDefinition.Method,
            MapTo = endpointDefinition.MapTo,
            ServiceName = endpointDefinition.ServiceConfig.Name,
            Meta = endpointDefinition.Meta.DistinctBy(meta => meta.Key).ToDictionary(a => a.Key, a => string.Join(";", a.Value))
        };
        EndpointDefinitions.TryAdd(key, containerEndpoint);
        return ValueTask.FromResult(key);
    }

    public ValueTask<ContainerEndpointDefinition?> GetAsync(DefinitionKey key, CancellationToken cancellationToken = default)
    {
        EndpointDefinitions.TryGetValue(key, out var value);
        return ValueTask.FromResult(value);
    }

    public ValueTask ClearAsync(CancellationToken cancellationToken = default)
    {
        EndpointDefinitions.Clear();
        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveAsync(DefinitionKey key, CancellationToken cancellationToken = default)
    {
        EndpointDefinitions.TryRemove(key, out _);
        return ValueTask.CompletedTask;
    }
}