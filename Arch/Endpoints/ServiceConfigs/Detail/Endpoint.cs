using Arch.Core.ServiceConfigs.Exceptions;
using Arch.Data.Abstractions.ServiceConfigs;
using FastEndpoints;
using FluentValidation;

namespace Arch.Endpoints.ServiceConfigs.Detail;

internal sealed class Endpoint : Endpoint<Request, ServiceConfigQueryResponse>
{
    private readonly IServiceConfigQuery _query;

    public Endpoint(IServiceConfigQuery query)
    {
        _query = query;
    }

    public override void Configure()
    {
        Get("service-configs/{id}");
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
            .NotEmpty().WithMessage("Enter ServiceConfig Id")
            .NotNull().WithMessage("Enter ServiceConfig Id");
    }
}