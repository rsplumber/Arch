using Arch.Core.ServiceConfigs.Exceptions;
using Arch.Data.Abstractions.ServiceConfigs;
using Microsoft.EntityFrameworkCore;

namespace Arch.Data.EF.ServiceConfigs;

internal sealed class ServiceConfigQuery : IServiceConfigQuery
{
    private readonly AppDbContext _dbContext;

    public ServiceConfigQuery(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<ServiceConfigQueryResponse> QueryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var serviceConfig = await _dbContext.ServiceConfigs
            .Select(config => new ServiceConfigQueryResponse
            {
                Id = config.Id,
                Name = config.Name,
                Primary = config.Primary,
                BaseUrl = config.BaseUrls.First(),
                Meta = config.Meta.ToDictionary(a => a.Key, a => string.Join(";", a.Value))
            })
            .FirstOrDefaultAsync(config => config.Id == id, cancellationToken: cancellationToken);

        if (serviceConfig is null) throw new ServiceConfigNotFoundException();

        return serviceConfig;
    }
}