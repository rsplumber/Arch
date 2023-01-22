﻿using Core.Domains;
using Microsoft.EntityFrameworkCore;

namespace Data.Sql;

public class ServiceConfigRepository : IServiceConfigRepository
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
            .ThenInclude(definition => definition.Meta)
            .FirstOrDefaultAsync(model => model.Id == id, cancellationToken);
    }

    public Task<ServiceConfig?> FindByEndpointAsync(string endpointPattern, CancellationToken cancellationToken = default)
    {
        return _dbContext.ServiceConfigs
            .Include(config => config.EndpointDefinitions)
            .ThenInclude(definition => definition.Meta)
            .FirstOrDefaultAsync(config => config.EndpointDefinitions.Any(definition => definition.Pattern == endpointPattern), cancellationToken);
    }
}