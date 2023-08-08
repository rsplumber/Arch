using Core.Entities.EndpointDefinitions;
using Core.Entities.EndpointDefinitions.Services;
using FastEndpoints;
using FluentValidation;

namespace Application.Endpoints.EndpointDefinitions.Delete;

internal sealed class Endpoint : Endpoint<Request>
{
    private readonly IEndpointDefinitionService _endpointDefinitionService;

    public Endpoint(IEndpointDefinitionService endpointDefinitionService)
    {
        _endpointDefinitionService = endpointDefinitionService;
    }

    public override void Configure()
    {
        Delete("endpoint-definitions/{id}");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        await _endpointDefinitionService.RemoveAsync(req.Id, ct);
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