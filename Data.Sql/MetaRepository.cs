using Core.ServiceConfigs;
using Microsoft.EntityFrameworkCore;

namespace Data.Sql;

public class MetaRepository : IMetaRepository
{
    private readonly ArchDbContext _dbContext;

    public MetaRepository(ArchDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Meta entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Metas.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Meta entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Metas.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Meta entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Metas.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Meta?> FindAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Metas
            .FirstOrDefaultAsync(model => model.Id == id, cancellationToken);
    }
}