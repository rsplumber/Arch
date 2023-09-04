using Arch.Core.EndpointDefinitions;
using Arch.Core.EndpointDefinitions.Exceptions;
using Arch.Core.Metas;
using FastEndpoints;
using FluentValidation;

namespace Arch.Authorization.Kundera.Endpoints.Permission;

internal sealed class Endpoint : Endpoint<Request>
{
    private readonly IEndpointDefinitionRepository _endpointDefinitionRepository;

    public Endpoint(IEndpointDefinitionRepository endpointDefinitionRepository)
    {
        _endpointDefinitionRepository = endpointDefinitionRepository;
    }


    public override void Configure()
    {
        Post("endpoint-definitions/{id}/security/permissions");
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

        endpointDefinition.RemoveMeta("allow_anonymous");
        endpointDefinition.RemoveMeta("permissions");
        endpointDefinition.Add(new Meta
        {
            Key = "permissions",
            Value = req.Permission.ToLower()
        });
        await _endpointDefinitionRepository.UpdateAsync(endpointDefinition, ct);

        await SendOkAsync(ct);
    }
}

internal sealed class Request
{
    public Guid Id { get; init; } = default!;

    public string Permission { get; init; } = default!;
}

internal sealed class RequestValidator : Validator<Request>
{
    public RequestValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty().WithMessage("Enter Id")
            .NotNull().WithMessage("Enter Id");

        RuleFor(request => request.Permission)
            .NotEmpty().WithMessage("Enter Permission")
            .NotNull().WithMessage("Enter Permission");
    }
}