using Core.Domains;
using FastEndpoints;
using FluentValidation;

namespace Management.Endpoints.ServiceConfigs.Create;

internal sealed class Endpoint : Endpoint<Request>
{
    private readonly IServiceConfigRepository _serviceConfigRepository;


    public Endpoint(IServiceConfigRepository serviceConfigRepository)
    {
        _serviceConfigRepository = serviceConfigRepository;
    }

    public override void Configure()
    {
        Post("service-configs");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var serviceConfig = new ServiceConfig
        {
            BaseUrl = req.BaseUrl,
            Name = req.Name,
            Meta = req.Meta.Select(pair => new Meta
            {
                Id = pair.Key,
                Value = pair.Value
            }).ToList()
        };
        await _serviceConfigRepository.AddAsync(serviceConfig, ct);
        await SendOkAsync(ct);
    }
}

internal class Request
{
    public string Name { get; set; } = default!;

    public string BaseUrl { get; set; } = default!;

    public Dictionary<string, string> Meta { get; set; } = new();
}

internal sealed class RequestValidator : Validator<Request>
{
    public RequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Enter Name")
            .NotNull().WithMessage("Enter Name");

        RuleFor(request => request.BaseUrl)
            .NotEmpty().WithMessage("Enter BaseUrl")
            .NotNull().WithMessage("Enter BaseUrl");
    }
}