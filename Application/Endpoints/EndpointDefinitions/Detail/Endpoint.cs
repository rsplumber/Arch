using Core.EndpointDefinitions.Exceptions;
using Data.EFCore;
using Data.Sql;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Endpoints.EndpointDefinitions.Detail;

internal sealed class Endpoint : Endpoint<Request>
{
    private readonly AppDbContext _dbContext;

    public Endpoint(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("endpoint-definitions/{id}");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var response = await _dbContext.EndpointDefinitions
            .Include(definition => definition.Meta)
            .Select(definition => new
            {
                definition.Id,
                definition.Method,
                definition.Pattern,
                definition.Endpoint,
                definition.MapTo,
                Meta = definition.Meta.Select(meta => new
                {
                    meta.Key,
                    meta.Value
                }).ToList()
            })
            .FirstOrDefaultAsync(definition => definition.Id == req.Id, ct);
        if (response is null)
        {
            throw new EndpointDefinitionNotFoundException();
        }

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
            .NotEmpty().WithMessage("Enter Id")
            .NotNull().WithMessage("Enter Id");
    }
}