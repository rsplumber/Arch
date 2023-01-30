using Data.Sql;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace Management.Endpoints.ServiceConfigs.List;

internal sealed class Endpoint : Endpoint<Request>
{
    private readonly AppDbContext _dbContext;

    public Endpoint(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("service-configs");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        var query = _dbContext.ServiceConfigs.AsQueryable();
        if (request.Name is not null)
        {
            query = query.Where(config => config.Name.Contains(request.Name));
        }

        var response = await query
            .Take(request.Size)
            .Skip(request.Size * (request.Page - 1))
            .Select(config => new
            {
                config.Id,
                config.Name
            }).ToListAsync(cancellationToken: ct);
        await SendOkAsync(response, ct);
    }
}

internal sealed record Request
{
    public string? Name { get; set; }

    public int Size { get; set; } = 10;

    public int Page { get; set; } = 1;
}