using Arch.Data.Abstractions.EndpointDefinitions;
using FastEndpoints;
using FluentValidation;

namespace Arch.Endpoints.EndpointDefinitions.Detail;

internal sealed class Endpoint : Endpoint<Request, EndpointDefinitionQueryResponse>
{
    private readonly IEndpointDefinitionQuery _query;

    public Endpoint(IEndpointDefinitionQuery query)
    {
        _query = query;
    }

    public override void Configure()
    {
        Get("endpoint-definitions/{id}");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var response = await _query.QueryAsync(req.Id, ct);
        await SendOkAsync(response, ct);
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