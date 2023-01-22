namespace Core.Domains;

public interface IEndpointDefinitionRepository
{
    Task AddAsync(EndpointDefinition entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(EndpointDefinition entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(EndpointDefinition entity, CancellationToken cancellationToken = default);

    Task<EndpointDefinition?> FindAsync(string pattern, CancellationToken cancellationToken = default);
}