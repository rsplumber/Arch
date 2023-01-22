namespace Core.ServiceConfigs;

public interface IBinderRepository
{
    Task AddAsync(Binder entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(Binder entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(Binder entity, CancellationToken cancellationToken = default);

    Task<Binder?> FindAsync(Guid id, CancellationToken cancellationToken = default);
}