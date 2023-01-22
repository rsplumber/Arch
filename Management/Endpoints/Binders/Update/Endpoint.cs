using Core.ServiceConfigs;
using FastEndpoints;
using FluentValidation;

namespace Management.Endpoints.Binders.Update;

internal sealed class Endpoint : Endpoint<UpdateBinderRequest>
{
    private readonly IBinderRepository _binderRepository;

    private readonly IServiceConfigRepository _serviceConfigRepository;


    public Endpoint(IBinderRepository binderRepository, IServiceConfigRepository serviceConfigRepository)
    {
        _binderRepository = binderRepository;
        _serviceConfigRepository = serviceConfigRepository;
    }

    public override void Configure()
    {
        Put("binder");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(UpdateBinderRequest req, CancellationToken ct)
    {
        var binder = await _binderRepository.FindAsync(req.Id, ct);
        var serviceConfig = await _serviceConfigRepository.FindByBinderAsync(binder.Id, ct);

        if (serviceConfig is null)
        {
            throw new Exception("ServiceConfig not found");
        }

        binder.ApiUrl = req.ApiUrl;
        binder.Metas.Clear();

        binder.Metas.Append(new Meta()
        {
            Id = "BaseUrl",
            Value = serviceConfig.BaseUrl
        });

        binder.Metas.Append(new Meta()
        {
            Id = "Secret",
            Value = serviceConfig.Secret
        });

        foreach (var meta in req.Metas)
        {
            binder.Metas.Append(new Meta
            {
                Id = meta.Key,
                Value = meta.Value
            });
        }


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

internal sealed class RequestValidator : Validator<UpdateBinderRequest>
{
    public RequestValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty().WithMessage("Enter Id")
            .NotNull().WithMessage("Enter Id");

        RuleFor(request => request.ApiUrl)
            .NotEmpty().WithMessage("Enter Name")
            .NotNull().WithMessage("Enter Name");
    }
}