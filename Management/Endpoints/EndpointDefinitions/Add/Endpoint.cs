using Core.EndpointDefinitions;
using FastEndpoints;
using FluentValidation;

namespace Management.Endpoints.EndpointDefinitions.Add;

internal sealed class Endpoint : Endpoint<Request>
{
    private readonly IEndpointDefinitionService _endpointDefinitionService;

    public Endpoint(IEndpointDefinitionService endpointDefinitionService)
    {
        _endpointDefinitionService = endpointDefinitionService;
    }

    public override void Configure()
    {
        Post("endpoint-definitions");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        await _endpointDefinitionService.AddAsync(new AddEndpointDefinitionRequest
        {
            Endpoint = req.Endpoint.StartsWith("/") ? req.Endpoint.Remove(0, 1) : req.Endpoint,
            Meta = req.Meta,
            ServiceConfigId = req.ServiceConfigId
        }, ct);
        await SendOkAsync(ct);
    }
}

internal class Request
{
    public Guid ServiceConfigId { get; set; }

    public string Endpoint { get; set; } = default!;

    public Dictionary<string, string> Meta { get; set; } = new();
}

internal sealed class RequestValidator : Validator<Request>
{
    public RequestValidator()
    {
        RuleFor(request => request.ServiceConfigId)
            .NotEmpty().WithMessage("Enter ServiceConfigId")
            .NotNull().WithMessage("Enter ServiceConfigId");

        RuleFor(request => request.Endpoint)
            .NotEmpty().WithMessage("Enter Endpoint")
            .NotNull().WithMessage("Enter Endpoint");
    }
}