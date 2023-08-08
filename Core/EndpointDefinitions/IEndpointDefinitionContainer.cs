namespace Core.EndpointDefinitions;

public interface IEndpointDefinitionContainer
{
    ValueTask<DefinitionKey> AddAsync(EndpointDefinition endpointDefinition, CancellationToken cancellationToken = default);

    ValueTask<ContainerEndpointDefinition?> GetAsync(DefinitionKey key, CancellationToken cancellationToken = default);

    ValueTask ClearAsync(CancellationToken cancellationToken = default);

    ValueTask RemoveAsync(DefinitionKey key, CancellationToken cancellationToken = default);
}