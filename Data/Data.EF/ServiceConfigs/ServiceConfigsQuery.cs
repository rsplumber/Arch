using Arch.Data.Abstractions.ServiceConfigs;
using Microsoft.EntityFrameworkCore;

namespace Arch.Data.EF.ServiceConfigs;

internal sealed class ServiceConfigsQuery : IServiceConfigsQuery
{
    private readonly AppDbContext _dbContext;

    public ServiceConfigsQuery(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<List<ServiceConfigsQueryResponse>> QueryAsync(string? name, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ServiceConfigs.AsQueryable();
        if (name is not null)
        {
            query = query.Where(config => config.Name.Contains(name));
        }

        return await query
            .OrderBy(config => config.CreatedAtUtc)
            .Select(config => new ServiceConfigsQueryResponse
            {
                Id = config.Id,
                Name = config.Name,
                Primary = config.Primary,
                BaseUrl = config.BaseUrls.First()
            }).ToListAsync(cancellationToken: cancellationToken);
    }
}