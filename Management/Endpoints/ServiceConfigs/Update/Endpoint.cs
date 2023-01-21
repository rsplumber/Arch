using Core.ServiceConfigs;
using FastEndpoints;
using FluentValidation;

namespace Management.Endpoints.ServiceConfigs.Update;

internal sealed class Endpoint : Endpoint<UpdateServiceConfigRequest>
{
    private readonly IServiceConfigRepository _serviceConfigRepository;


    public Endpoint(IServiceConfigRepository serviceConfigRepository)
    {
        _serviceConfigRepository = serviceConfigRepository;
    }

    public override void Configure()
    {
        Put("service-configs");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(UpdateServiceConfigRequest req, CancellationToken ct)
    {
        var serviceConfig = await _serviceConfigRepository.FindAsync(req.Id, ct);
        if (serviceConfig is null)
        {
            throw new Exception("ServiceConfig not found");
        }

        serviceConfig.Name = req.Name;
        serviceConfig.BaseUrl = req.BaseUrl;
        serviceConfig.Secret = req.Secret;

        await _serviceConfigRepository.UpdateAsync(serviceConfig, ct);
        await SendOkAsync(ct);
    }
}

internal sealed class EndpointSummary : Summary<Endpoint>
{
    public EndpointSummary()
    {
        Summary = "Update ServiceConfig in the system";
        Description = "Update ServiceConfig in the system";
        Response(200, "ServiceConfig was successfully updated");
    }
}

internal sealed class RequestValidator : Validator<UpdateServiceConfigRequest>
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


        RuleFor(request => request.Secret)
            .NotEmpty().WithMessage("Enter Secret")
            .NotNull().WithMessage("Enter Secret");
    }
}