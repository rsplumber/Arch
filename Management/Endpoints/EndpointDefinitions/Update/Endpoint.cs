using Core.EndpointDefinitions.Services;
using FastEndpoints;
using FluentValidation;

namespace Management.Endpoints.EndpointDefinitions.Update;

internal sealed class Endpoint : Endpoint<Request>
{
    private readonly IEndpointDefinitionService _endpointDefinitionService;

    public Endpoint(IEndpointDefinitionService endpointDefinitionService)
    {
        _endpointDefinitionService = endpointDefinitionService;
    }

    public override void Configure()
    {
        Put("endpoint-definitions/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        await _endpointDefinitionService.UpdateAsync(new UpdateEndpointDefinitionRequest
        {
            Id = req.Id,
            Meta = req.Meta,
        }, ct);

        await SendOkAsync(ct);
    }
}

internal class Request
{
    public Guid Id { get; set; } = default!;

    public Dictionary<string, string> Meta { get; set; } = new();
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