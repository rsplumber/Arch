using Core;
using Core.ServiceConfigs;
using FastEndpoints;
using FluentValidation;

namespace Management.Endpoints.Binders.Add;

internal sealed class Endpoint : Endpoint<AddBinderRequest>
{
    private readonly IServiceConfigRepository _serviceConfigRepository;
    private readonly IBinderRepository _binderRepository;


    public Endpoint(IServiceConfigRepository serviceConfigRepository, IBinderRepository binderRepository)
    {
        _serviceConfigRepository = serviceConfigRepository;
        _binderRepository = binderRepository;
    }

    public override void Configure()
    {
        Post("binder");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(AddBinderRequest req, CancellationToken ct)
    {
        var serviceConfig = await _serviceConfigRepository.FindAsync(req.ServiceConfigId, ct);
        if (serviceConfig is null)
        {
            throw new Exception("ServiceConfig not found");
        }

        BaseTree.BaseTreeNode.Append(req.Bind);
        var binderId = await BaseTree.BaseTreeNode.FindAsync(req.Bind, ct);

        var metas = new List<Meta>();

        metas.Append(new Meta()
        {
            Id = "BaseUrl",
            Value = serviceConfig.BaseUrl
        });

        metas.Append(new Meta()
        {
            Id = "Secret",
            Value = serviceConfig.Secret
        });

        foreach (var meta in req.Metas)
        {
            metas.Append(new Meta
            {
                Id = meta.Key,
                Value = meta.Value
            });
        }


        var binder = new Binder()
        {
            Id = binderId,
            ApiUrl = req.ApiUrl,
            Metas = metas
        };

        await _binderRepository.AddAsync(binder, ct);
        await SendOkAsync(ct);
    }
}

internal sealed class EndpointSummary : Summary<Endpoint>
{
    public EndpointSummary()
    {
        Summary = "Create Binder in the system";
        Description = "Create Binder in the system";
        Response(200, "Binder was successfully created");
    }
}

internal sealed class RequestValidator : Validator<AddBinderRequest>
{
    public RequestValidator()
    {
        RuleFor(request => request.Bind)
            .NotEmpty().WithMessage("Enter Bind")
            .NotNull().WithMessage("Enter Bind");

        RuleFor(request => request.ApiUrl)
            .NotEmpty().WithMessage("Enter ApiUrl")
            .NotNull().WithMessage("Enter ApiUrl");


        RuleFor(request => request.ServiceConfigId)
            .NotEmpty().WithMessage("Enter ServiceConfigId")
            .NotNull().WithMessage("Enter ServiceConfigId");
    }
}