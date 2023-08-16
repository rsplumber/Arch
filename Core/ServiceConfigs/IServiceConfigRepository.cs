namespace Core.ServiceConfigs;

public interface IServiceConfigRepository
{
    Task AddAsync(ServiceConfig entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(ServiceConfig entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(ServiceConfig entity, CancellationToken cancellationToken = default);

    Task<ServiceConfig?> FindAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<ServiceConfig?> FindByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<List<ServiceConfig>> FindAsync(CancellationToken cancellationToken = default);

    Task<ServiceConfig?> FindByEndpointAsync(Guid endpointId, CancellationToken cancellationToken = default);
}