using Core.Domains;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Management.Endpoints.ServiceConfigs.Detail;

internal sealed class Endpoint : Endpoint<Request, ServiceConfig>
{
    private readonly ManagementDbContext _dbContext;

    public Endpoint(ManagementDbContext dbContext)
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
            .Include(config => config.EndpointDefinitions)
            .ThenInclude(definition => definition.Meta)
            .FirstAsync(config => config.Id == req.Id, cancellationToken: ct);
        await SendOkAsync(response, ct);
    }
}

internal class Request
{
    public Guid Id { get; set; }
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