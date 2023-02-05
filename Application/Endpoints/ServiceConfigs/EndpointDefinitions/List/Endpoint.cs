using Core.ServiceConfigs.Exceptions;
using Data.Sql;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Endpoints.ServiceConfigs.EndpointDefinitions.List;

internal sealed class Endpoint : Endpoint<Request>
{
    private readonly AppDbContext _dbContext;

    public Endpoint(AppDbContext dbContext)
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
        if (serviceConfig is null)
        {
            throw new ServiceConfigNotFoundException();
        }

        if (req.Endpoint is not null)
        {
            serviceConfig.EndpointDefinitions = serviceConfig.EndpointDefinitions
                .Where(definition => definition.Endpoint.Contains(req.Endpoint))
                .ToList();
        }

        await SendOkAsync(serviceConfig.EndpointDefinitions.Select(definition => new
        {
            definition.Id,
            definition.Endpoint,
            definition.Pattern,
            definition.Method
        }).ToList(), ct);
    }
}

internal class Request
{
    public Guid Id { get; set; }

    public string? Endpoint { get; set; }
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