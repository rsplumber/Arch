using Core.EndpointDefinitions;
using Microsoft.EntityFrameworkCore;

namespace Data.EFCore;

public sealed class EndpointDefinitionRepository : IEndpointDefinitionRepository
{
    private readonly AppDbContext _dbContext;

    public EndpointDefinitionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(EndpointDefinition entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.EndpointDefinitions.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(EndpointDefinition entity, CancellationToken cancellationToken = default)
    {
        _dbContext.EndpointDefinitions.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(EndpointDefinition entity, CancellationToken cancellationToken = default)
    {
        _dbContext.EndpointDefinitions.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<EndpointDefinition?> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.EndpointDefinitions
            .Include(definition => definition.ServiceConfig)
            .FirstOrDefaultAsync(model => model.Id == id, cancellationToken);
    }
}