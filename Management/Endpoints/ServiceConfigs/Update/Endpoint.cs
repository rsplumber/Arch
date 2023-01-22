using Core.Domains;
using FastEndpoints;
using FluentValidation;

namespace Management.Endpoints.ServiceConfigs.Update;

internal sealed class Endpoint : Endpoint<Request>
{
    private readonly IServiceConfigRepository _serviceConfigRepository;


    public Endpoint(IServiceConfigRepository serviceConfigRepository)
    {
        _serviceConfigRepository = serviceConfigRepository;
    }

    public override void Configure()
    {
        Put("service-configs/{id}");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var serviceConfig = await _serviceConfigRepository.FindAsync(req.Id, ct);
        if (serviceConfig is null)
        {
            throw new Exception("ServiceConfig not found");
        }

        serviceConfig.Name = req.Name;
        serviceConfig.BaseUrl = req.BaseUrl;

        serviceConfig.Meta.Clear();

        foreach (var meta in req.Meta)
        {
            serviceConfig.Meta.Add(new Meta
            {
                Id = meta.Key,
                Value = meta.Value
            });
        }

        await _serviceConfigRepository.UpdateAsync(serviceConfig, ct);
        await SendOkAsync(ct);
    }
}

public class Request
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string BaseUrl { get; set; } = default!;

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

        RuleFor(request => request.BaseUrl)
            .NotEmpty().WithMessage("Enter BaseUrl")
            .NotNull().WithMessage("Enter BaseUrl");
    }
}