using Core.Domains;
using FastEndpoints;
using FluentValidation;

namespace Management.Endpoints.ServiceConfigs.Delete;

internal sealed class Endpoint : Endpoint<Request>
{
    private readonly IServiceConfigRepository _serviceConfigRepository;


    public Endpoint(IServiceConfigRepository serviceConfigRepository)
    {
        _serviceConfigRepository = serviceConfigRepository;
    }

    public override void Configure()
    {
        Delete("service-configs/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var serviceConfig = await _serviceConfigRepository.FindAsync(req.Id, ct);
        if (serviceConfig is null)
        {
            throw new Exception("ServiceConfig not found");
        }

        await _serviceConfigRepository.DeleteAsync(serviceConfig, ct);
        await SendOkAsync(ct);
    }
}

public class Request
{
    public Guid Id { get; set; }
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