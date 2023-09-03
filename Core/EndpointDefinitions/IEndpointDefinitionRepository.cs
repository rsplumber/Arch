namespace Arch.Core.EndpointDefinitions;

public interface IEndpointDefinitionRepository
{
    Task AddAsync(EndpointDefinition entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(EndpointDefinition entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(EndpointDefinition entity, CancellationToken cancellationToken = default);

    Task<EndpointDefinition?> FindAsync(Guid id, CancellationToken cancellationToken = default);

    Task<EndpointDefinition?> FindAsync(DefinitionKey definitionKey, CancellationToken cancellationToken = default);
}