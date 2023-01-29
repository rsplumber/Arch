using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using EndpointDefinition = Core.EndpointDefinitions.EndpointDefinition;

namespace Management.Endpoints.EndpointDefinitions.Detail;

internal sealed class Endpoint : FastEndpoints.Endpoint<Request, EndpointDefinition>
{
    private readonly ManagementDbContext _dbContext;

    public Endpoint(ManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("endpoint-definitions/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var response = await _dbContext.EndpointDefinitions
            .Include(definition => definition.Meta)
            .FirstAsync(definition => definition.Id == req.Id, ct);
        await SendOkAsync(response, ct);
    }
}

internal class Request
{
    public Guid Id { get; set; } = default!;
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