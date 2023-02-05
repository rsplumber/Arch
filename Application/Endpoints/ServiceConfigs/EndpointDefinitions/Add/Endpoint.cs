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

internal class Request
{
    public Guid Id { get; set; }

    public string Endpoint { get; set; } = default!;

    public string Method { get; set; } = default!;

    public Dictionary<string, string> Meta { get; set; } = new();
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