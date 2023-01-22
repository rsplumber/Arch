using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace Management.Endpoints.EndpointDefinitions.List;

internal sealed class Endpoint : EndpointWithoutRequest
{
    private readonly ManagementDbContext _dbContext;

    public Endpoint(ManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("endpoint-definitions");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = await _dbContext.EndpointDefinitions
            .Select(definition => definition).ToListAsync(cancellationToken: ct);
        await SendOkAsync(response, ct);
    }
}