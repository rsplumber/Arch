using Core.ServiceConfigs;
using FastEndpoints;
using FluentValidation;

namespace Management.Endpoints.ServiceConfigs.Delete;

internal sealed class Endpoint : Endpoint<DeleteServiceConfigRequest>
{
    private readonly IServiceConfigRepository _serviceConfigRepository;


    public Endpoint(IServiceConfigRepository serviceConfigRepository)
    {
        _serviceConfigRepository = serviceConfigRepository;
    }

    public override void Configure()
    {
        Delete("service-configs");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(DeleteServiceConfigRequest req, CancellationToken ct)
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

internal sealed class EndpointSummary : Summary<Endpoint>
{
    public EndpointSummary()
    {
        Summary = "Delete ServiceConfig in the system";
        Description = "Delete ServiceConfig in the system";
        Response(200, "ServiceConfig was successfully Deleted");
    }
}

internal sealed class RequestValidator : Validator<DeleteServiceConfigRequest>
{
    public RequestValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty().WithMessage("Enter Id")
            .NotNull().WithMessage("Enter Id");
    }
}