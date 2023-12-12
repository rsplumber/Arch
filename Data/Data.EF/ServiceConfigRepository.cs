using Arch.Core.ServiceConfigs;
using Microsoft.EntityFrameworkCore;

namespace Arch.Data.EF;

public sealed class ServiceConfigRepository : IServiceConfigRepository
{
    private readonly AppDbContext _dbContext;

    public ServiceConfigRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ServiceConfig entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.ServiceConfigs.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ServiceConfig entity, CancellationToken cancellationToken = default)
    {
        _dbContext.ServiceConfigs.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ServiceConfig entity, CancellationToken cancellationToken = default)
    {
        _dbContext.ServiceConfigs.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ServiceConfig?> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceConfigs
            .Include(config => config.EndpointDefinitions)
            .FirstOrDefaultAsync(model => model.Id == id, cancellationToken);
    }

    public async Task<ServiceConfig?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceConfigs
            .Include(config => config.EndpointDefinitions)
            .FirstOrDefaultAsync(model => model.Name == name, cancellationToken);
    }

    public async Task<List<ServiceConfig>> FindAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceConfigs
            .Include(config => config.EndpointDefinitions)
            .ToListAsync(cancellationToken);
    }

    public Task<ServiceConfig?> FindByEndpointAsync(Guid endpointId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ServiceConfigs
            .Include(config => config.EndpointDefinitions)
            .FirstOrDefaultAsync(config => config.EndpointDefinitions.Any(definition => definition.Id == endpointId), cancellationToken);
    }
}