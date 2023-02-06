using Core.ServiceConfigs.Services;
using FastEndpoints;
using FluentValidation;

namespace Application.Endpoints.ServiceConfigs.Delete;

internal sealed class Endpoint : Endpoint<Request>
{
    private readonly IServiceConfigService _serviceConfigService;

    public Endpoint(IServiceConfigService serviceConfigService)
    {
        _serviceConfigService = serviceConfigService;
    }

    public override void Configure()
    {
        Delete("service-configs/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        await _serviceConfigService.DeleteAsync(req.Id, ct);
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