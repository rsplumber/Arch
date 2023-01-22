using Core.ServiceConfigs;
using Microsoft.EntityFrameworkCore;

namespace Data.Sql;

public class BinderRepository : IBinderRepository
{
    private readonly ArchDbContext _dbContext;

    public BinderRepository(ArchDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Binder entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Binders.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Binder entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Binders.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Binder entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Binders.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Binder?> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Binders
            .FirstOrDefaultAsync(model => model.Id == id, cancellationToken);
    }
}