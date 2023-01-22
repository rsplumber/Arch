using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using EndpointDefinition = Core.Domains.EndpointDefinition;

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
        Get("endpoint-definitions/{pattern}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var response = await _dbContext.EndpointDefinitions
            .Include(definition => definition.Meta)
            .FirstAsync(definition => definition.Pattern == req.Pattern, cancellationToken: ct);
        await SendOkAsync(response, ct);
    }
}

internal class Request
{
    public string Pattern { get; set; } = default!;
}

internal sealed class RequestValidator : Validator<Request>
{
    public RequestValidator()
    {
        RuleFor(request => request.Pattern)
            .NotEmpty().WithMessage("Enter Pattern")
            .NotNull().WithMessage("Enter Pattern");
    }
}