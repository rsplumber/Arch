using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace Management.Endpoints.ServiceConfigs.List;

internal sealed class Endpoint : EndpointWithoutRequest
{
    private readonly ManagementDbContext _dbContext;

    public Endpoint(ManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("service-configs");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = await _dbContext.ServiceConfigs
            .Select(config => config).ToListAsync(cancellationToken: ct);
        await SendOkAsync(response, ct);
    }
}