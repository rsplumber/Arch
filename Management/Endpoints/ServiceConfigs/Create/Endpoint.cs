using Core.ServiceConfigs.Services;
using FastEndpoints;
using FluentValidation;

namespace Management.Endpoints.ServiceConfigs.Create;

internal sealed class Endpoint : Endpoint<Request>
{
    private readonly IServiceConfigService _serviceConfigService;

    public Endpoint(IServiceConfigService serviceConfigService)
    {
        _serviceConfigService = serviceConfigService;
    }

    public override void Configure()
    {
        Post("service-configs");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        await _serviceConfigService.CreateAsync(new CreateServiceConfigRequest
        {
            Name = req.Name,
            Meta = req.Meta
        }, ct);
        await SendOkAsync(ct);
    }
}

internal class Request
{
    public string Name { get; set; } = default!;

    public Dictionary<string, string> Meta { get; set; } = new();
}

internal sealed class RequestValidator : Validator<Request>
{
    public RequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Enter Name")
            .NotNull().WithMessage("Enter Name");
    }
}