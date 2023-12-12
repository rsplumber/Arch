using Arch.Core.ServiceConfigs.EndpointDefinitions;
using Arch.Core.ServiceConfigs.EndpointDefinitions.Exceptions;
using FastEndpoints;
using FluentValidation;

namespace Arch.Authorization.Kundera.Endpoints.AllowAnonymous;

internal sealed class Endpoint : Endpoint<Request>
{
    private readonly IEndpointDefinitionRepository _endpointDefinitionRepository;

    public Endpoint(IEndpointDefinitionRepository endpointDefinitionRepository)
    {
        _endpointDefinitionRepository = endpointDefinitionRepository;
    }

    public override void Configure()
    {
        Post("endpoint-definitions/{id}/security/allow-anonymous");
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

        endpointDefinition.RemoveMeta("permissions");
        endpointDefinition.RemoveMeta("allow_anonymous");
        endpointDefinition.AddMeta("allow_anonymous", "true");
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