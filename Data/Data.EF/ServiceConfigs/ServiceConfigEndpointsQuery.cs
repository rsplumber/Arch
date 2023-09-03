using Arch.Core.ServiceConfigs.Exceptions;
using Arch.Data.Abstractions.ServiceConfigs;
using Microsoft.EntityFrameworkCore;

namespace Arch.Data.EF.ServiceConfigs;

internal sealed class ServiceConfigEndpointsQuery : IServiceConfigEndpointsQuery
{
    private readonly AppDbContext _dbContext;

    public ServiceConfigEndpointsQuery(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<List<ServiceConfigEndpointsQueryResponse>> QueryAsync(Guid id, string? endpoint, CancellationToken cancellationToken = default)
    {
        var serviceConfig = await _dbContext.ServiceConfigs
            .Include(config => config.EndpointDefinitions)
            .FirstOrDefaultAsync(config => config.Id == id, cancellationToken: cancellationToken);

        if (serviceConfig is null) throw new ServiceConfigNotFoundException();

        if (endpoint is not null)
        {
            serviceConfig.EndpointDefinitions = serviceConfig.EndpointDefinitions
                .Where(definition => definition.Endpoint.Contains(endpoint))
                .ToList();
        }

        return serviceConfig.EndpointDefinitions.Select(definition => new ServiceConfigEndpointsQueryResponse
        {
            Id = definition.Id,
            Endpoint = definition.Endpoint,
            Pattern = definition.Pattern,
            Method = definition.Method.ToString(),
            MapTo = definition.MapTo
        }).ToList();
    }
}