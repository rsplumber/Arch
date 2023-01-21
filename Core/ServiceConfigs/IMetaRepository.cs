namespace Core.ServiceConfigs;

public interface IMetaRepository
{
    Task AddAsync(Meta entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(Meta entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(Meta entity, CancellationToken cancellationToken = default);

    Task<Meta?> FindAsync(string id, CancellationToken cancellationToken = default);
}