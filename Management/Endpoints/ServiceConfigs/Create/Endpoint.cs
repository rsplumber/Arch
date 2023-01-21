using Core.ServiceConfigs;
using FastEndpoints;
using FluentValidation;

namespace Management.Endpoints.ServiceConfigs.Create;

internal sealed class Endpoint : Endpoint<CreateServiceConfigRequest>
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

    public override async Task HandleAsync(CreateServiceConfigRequest req, CancellationToken ct)
    {
        var serviceConfig = new ServiceConfig()
        {
            BaseUrl = req.BaseUrl,
            Name = req.Name,
            Secret = req.Secret
        };

        await _serviceConfigRepository.AddAsync(serviceConfig, ct);
        await SendOkAsync(ct);
    }
}

internal sealed class EndpointSummary : Summary<Endpoint>
{
    public EndpointSummary()
    {
        Summary = "Create ServiceConfig in the system";
        Description = "Create ServiceConfig in the system";
        Response(200, "ServiceConfig was successfully created");
    }
}

internal sealed class RequestValidator : Validator<CreateServiceConfigRequest>
{
    public RequestValidator()
    {
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