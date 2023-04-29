using Core.EndpointDefinitions.Exceptions;
using Core.EndpointDefinitions.Services;
using Data.Sql;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Endpoints.EndpointDefinitions.Enable;

internal sealed class Endpoint : Endpoint<Request>
{
    private readonly IEndpointDefinitionService _endpointDefinitionService;
    private readonly AppDbContext _dbContext;


    public Endpoint(IEndpointDefinitionService endpointDefinitionService, AppDbContext dbContext)
    {
        _endpointDefinitionService = endpointDefinitionService;
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Patch("endpoint-definitions/{id}/enable");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var endpointDefinition = await _dbContext.EndpointDefinitions.FirstOrDefaultAsync(definition => definition.Id == req.Id, ct);
        if (endpointDefinition is null)
        {
            throw new EndpointDefinitionNotFoundException();
        }

        var meta = endpointDefinition.Meta.ToDictionary(a => a.Key, a => string.Join(";", a.Value!));
        meta.Remove("disabled");
        await _endpointDefinitionService.UpdateAsync(new UpdateEndpointDefinitionRequest
        {
            Id = req.Id,
            Meta = meta,
        }, ct);

        await SendOkAsync(ct);
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
            .NotEmpty().WithMessage("Enter Id")
            .NotNull().WithMessage("Enter Id");
    }
}