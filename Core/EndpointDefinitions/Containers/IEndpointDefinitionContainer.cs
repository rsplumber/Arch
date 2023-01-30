namespace Core.EndpointDefinitions.Containers;

public interface IEndpointDefinitionContainer
{
    ValueTask AddAsync(EndpointDefinition endpointDefinition, CancellationToken cancellationToken = default);

    ValueTask<EndpointDefinition?> GetAsync(DefinitionKey key, CancellationToken cancellationToken = default);

    EndpointDefinition? Get(DefinitionKey key);

    void Clear();

    ValueTask RemoveAsync(DefinitionKey key, CancellationToken cancellationToken = default);
}