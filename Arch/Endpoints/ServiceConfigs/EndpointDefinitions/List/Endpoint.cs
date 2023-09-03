using Arch.Data.Abstractions.ServiceConfigs;
using FastEndpoints;
using FluentValidation;

namespace Arch.Endpoints.ServiceConfigs.EndpointDefinitions.List;

internal sealed class Endpoint : Endpoint<Request>
{
    private readonly IServiceConfigEndpointsQuery _query;

    public Endpoint(IServiceConfigEndpointsQuery query)
    {
        _query = query;
    }

    public override void Configure()
    {
        Get("service-configs/{id}/endpoint-definitions");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var response = await _query.QueryAsync(req.Id, req.Endpoint, ct);
        await SendOkAsync(response, ct);
    }
}

internal sealed class Request
{
    public Guid Id { get; init; } = default!;

    public string? Endpoint { get; init; } = default;
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