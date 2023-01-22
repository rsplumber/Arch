using Core.ServiceConfigs;
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
        Version(1);
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var serviceConfig = new ServiceConfig()
        {
            BaseUrl = req.BaseUrl,
            Name = req.Name,
        };

        foreach (var meta in req.Meta)
        {
            serviceConfig.Meta.Add(new Meta
            {
                Id = meta.Key,
                Value = meta.Value
            });
        }

        await _serviceConfigRepository.AddAsync(serviceConfig, ct);
        await SendOkAsync(ct);
    }
}

internal class Request
{
    public string Name { get; set; } 
    
    public string BaseUrl { get; set; }

    public Dictionary<string, string> Meta { get; set; }
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