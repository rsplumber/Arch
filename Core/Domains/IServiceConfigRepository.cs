namespace Core.Domains;

public interface IServiceConfigRepository
{
    Task AddAsync(ServiceConfig entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(ServiceConfig entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(ServiceConfig entity, CancellationToken cancellationToken = default);

    Task<ServiceConfig?> FindAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ServiceConfig?> FindByEndpointAsync(string endpointPattern, CancellationToken cancellationToken = default);
}