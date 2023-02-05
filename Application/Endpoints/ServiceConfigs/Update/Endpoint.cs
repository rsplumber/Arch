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
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        await _serviceConfigService.UpdateAsync(new UpdateServiceConfigRequest
        {
            Id = req.Id,
            Name = req.Name,
            Meta = req.Meta
        }, ct);
        await SendOkAsync(ct);
    }
}

public class Request
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public Dictionary<string, string> Meta { get; set; } = new();
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
    }
}