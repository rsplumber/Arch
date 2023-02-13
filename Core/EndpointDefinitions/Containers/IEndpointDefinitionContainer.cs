namespace Core.EndpointDefinitions.Containers;

public interface IEndpointDefinitionContainer
{
    ValueTask<DefinitionKey> AddAsync(EndpointDefinition endpointDefinition, CancellationToken cancellationToken = default);

    ValueTask<ContainerEndpointDefinition?> GetAsync(DefinitionKey key, CancellationToken cancellationToken = default);

    ContainerEndpointDefinition? Get(DefinitionKey key);

    void Clear();

    ValueTask RemoveAsync(DefinitionKey key, CancellationToken cancellationToken = default);
}