using Arch.Core.EndpointDefinitions;
using Arch.Core.EndpointDefinitions.Exceptions;
using FastEndpoints;
using FluentValidation;

namespace Arch.Endpoints.EndpointDefinitions.Enable;

internal sealed class Endpoint : Endpoint<Request>
{
    private readonly IEndpointDefinitionRepository _endpointDefinitionRepository;

    public Endpoint(IEndpointDefinitionRepository endpointDefinitionRepository)
    {
        _endpointDefinitionRepository = endpointDefinitionRepository;
    }

    public override void Configure()
    {
        Patch("endpoint-definitions/{id}/enable");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var endpointDefinition = await _endpointDefinitionRepository.FindAsync(req.Id, ct);
        if (endpointDefinition is null)
        {
            throw new EndpointDefinitionNotFoundException();
        }

        endpointDefinition.Enable();

        await _endpointDefinitionRepository.UpdateAsync(endpointDefinition, ct);

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