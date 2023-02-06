using Core.EndpointDefinitions.Services;
using FastEndpoints;
using FluentValidation;

namespace Application.Endpoints.ServiceConfigs.EndpointDefinitions.Add;

internal sealed class Endpoint : Endpoint<Request>
{
    private readonly IEndpointDefinitionService _endpointDefinitionService;

    public Endpoint(IEndpointDefinitionService endpointDefinitionService)
    {
        _endpointDefinitionService = endpointDefinitionService;
    }

    public override void Configure()
    {
        Post("service-configs/{id}/endpoint-definitions");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        await _endpointDefinitionService.AddAsync(new AddEndpointDefinitionRequest
        {
            Endpoint = req.Endpoint,
            Method = req.Method,
            Meta = req.Meta,
            ServiceConfigId = req.Id
        }, ct);
        await SendOkAsync(ct);
    }
}

internal sealed class Request
{
    public Guid Id { get; init; } = default!;

    public string Endpoint { get; init; } = default!;

    public string Method { get; init; } = default!;

    public Dictionary<string, string> Meta { get; init; } = new();
}

internal sealed class RequestValidator : Validator<Request>
{
    public RequestValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty().WithMessage("Enter ServiceConfigId")
            .NotNull().WithMessage("Enter ServiceConfigId");

        RuleFor(request => request.Endpoint)
            .NotEmpty().WithMessage("Enter Endpoint")
            .NotNull().WithMessage("Enter Endpoint");
    }
}