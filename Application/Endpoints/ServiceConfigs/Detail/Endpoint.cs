using Data.Sql;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Endpoints.ServiceConfigs.Detail;

internal sealed class Endpoint : Endpoint<Request>
{
    private readonly AppDbContext _dbContext;

    public Endpoint(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("service-configs/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var response = await _dbContext.ServiceConfigs
            .Include(config => config.Meta)
            .Select(config => new
            {
                config.Id,
                config.Name,
                Meta = config.Meta.Select(meta => new
                {
                    meta.Id,
                    meta.Key,
                    meta.Value
                }).ToList()
            })
            .FirstAsync(config => config.Id == req.Id, cancellationToken: ct);
        await SendOkAsync(response, ct);
    }
}

internal sealed class Request
{
    public Guid Id { get; init; } = default!;
}

internal sealed class RequestValidator : Validator<Request>
{
    public RequestValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty().WithMessage("Enter ServiceConfig Id")
            .NotNull().WithMessage("Enter ServiceConfig Id");
    }
}