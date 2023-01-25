using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Management.Endpoints.ServiceConfigs.EndpointDefinitions.List;

internal sealed class Endpoint : Endpoint<Request>
{
    private readonly ManagementDbContext _dbContext;

    public Endpoint(ManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("service-configs/{id}/endpoint-definitions");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var serviceConfig = await _dbContext.ServiceConfigs
            .Include(config => config.EndpointDefinitions)
            .FirstOrDefaultAsync(config => config.Id == req.Id, cancellationToken: ct);
        var response = serviceConfig.EndpointDefinitions.Select(definition => new
        {
            definition.Endpoint,
            definition.Pattern
        }).ToList();
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
            .NotEmpty().WithMessage("Enter Id")
            .NotNull().WithMessage("Enter Id");
    }
}