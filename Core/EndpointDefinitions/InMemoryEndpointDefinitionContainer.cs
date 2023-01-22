using System.Collections.Concurrent;
using Core.Domains;

namespace Core.EndpointDefinitions;

public class InMemoryEndpointDefinitionContainer : IEndpointDefinitionContainer
{
    private static readonly ConcurrentDictionary<string, EndpointDefinition> EndpointDefinitions = new();

    public ValueTask AddAsync(EndpointDefinition endpointDefinition, CancellationToken cancellationToken = default)
    {
        EndpointDefinitions.TryAdd(endpointDefinition.Pattern, endpointDefinition);
        return ValueTask.CompletedTask;
    }

    public ValueTask<EndpointDefinition?> GetAsync(string pattern, CancellationToken cancellationToken = default)
    {
        EndpointDefinitions.TryGetValue(pattern, out var value);
        return ValueTask.FromResult(value);
    }

    public EndpointDefinition? Get(string pattern)
    {
        EndpointDefinitions.TryGetValue(pattern, out var definition);
        return definition;
    }

    public ValueTask RemoveAsync(string pattern, CancellationToken cancellationToken = default)
    {
        EndpointDefinitions.TryRemove(pattern, out _);
        return ValueTask.CompletedTask;
    }
}