using Core.Binders;

namespace Core.ServiceConfigs;

public interface IServiceConfigRepository
{
    Task AddAsync(ServiceConfig entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(ServiceConfig entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(ServiceConfig entity, CancellationToken cancellationToken = default);

    Task<Binder?> FindAsync(string id, CancellationToken cancellationToken = default);
}