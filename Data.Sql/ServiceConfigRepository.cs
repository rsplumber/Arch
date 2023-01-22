﻿using Core.ServiceConfigs;
using Microsoft.EntityFrameworkCore;

namespace Data.Sql;

public class ServiceConfigRepository : IServiceConfigRepository
{
    private readonly ArchDbContext _dbContext;

    public ServiceConfigRepository(ArchDbContext dbContext)
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
            .FirstOrDefaultAsync(model => model.Id == id, cancellationToken);
    }

    public async Task<ServiceConfig?> FindByBinderAsync(string binderId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceConfigs
            .FirstOrDefaultAsync(config => config.Binders.Any(binder => binder.Id == binderId), cancellationToken: cancellationToken);
    }
}