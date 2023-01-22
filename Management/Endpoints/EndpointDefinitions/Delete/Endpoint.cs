using Core.EndpointDefinitions;
using FastEndpoints;
using FluentValidation;

namespace Management.Endpoints.EndpointDefinitions.Delete;

internal sealed class Endpoint : Endpoint<Request>
{
    private readonly IEndpointDefinitionService _endpointDefinitionService;

    public Endpoint(IEndpointDefinitionService endpointDefinitionService)
    {
        _endpointDefinitionService = endpointDefinitionService;
    }

    public override void Configure()
    {
        Delete("endpoint-definitions/{pattern}");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        await _endpointDefinitionService.RemoveAsync(req.Pattern, ct);
        await SendOkAsync(ct);
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