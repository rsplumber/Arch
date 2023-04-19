using Core.ServiceConfigs.Services;
using FastEndpoints;
using FluentValidation;

namespace Application.Endpoints.ServiceConfigs.Update;

internal sealed class Endpoint : Endpoint<Request>
{
    private readonly IServiceConfigService _serviceConfigService;

    public Endpoint(IServiceConfigService serviceConfigService)
    {
        _serviceConfigService = serviceConfigService;
    }

    public override void Configure()
    {
        Put("service-configs/{id}");
        // Permissions("arch_service-configs_update");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        await _serviceConfigService.UpdateAsync(new UpdateServiceConfigRequest
        {
            Id = req.Id,
            Name = req.Name,
            Meta = req.Meta,
            BaseUrl = req.BaseUrl
        }, ct);
        await SendOkAsync(ct);
    }
}

internal sealed class Request
{
    public Guid Id { get; init; } = default;

    public string Name { get; init; } = default!;

    public string BaseUrl { get; init; } = default!;

    public Dictionary<string, string> Meta { get; init; } = new();
}

internal sealed class RequestValidator : Validator<Request>
{
    public RequestValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty().WithMessage("Enter Id")
            .NotNull().WithMessage("Enter Id");

        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Enter Name")
            .NotNull().WithMessage("Enter Name");

        RuleFor(request => request.BaseUrl)
            .NotEmpty().WithMessage("Enter BaseUrl")
            .NotNull().WithMessage("Enter BaseUrl");
    }
}