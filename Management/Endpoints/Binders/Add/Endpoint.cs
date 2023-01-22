using Core;
using Core.ServiceConfigs;
using FastEndpoints;
using FluentValidation;

namespace Management.Endpoints.Binders.Add;

internal sealed class Endpoint : Endpoint<Request>
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

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var serviceConfig = await _serviceConfigRepository.FindAsync(req.ServiceConfigId, ct);
        if (serviceConfig is null)
        {
            throw new Exception("ServiceConfig not found");
        }

        BaseTree.BaseTreeNode.Append(req.Bind);
        var bind = await BaseTree.BaseTreeNode.FindAsync(req.Bind, ct);

        var metas = new List<Meta>();

        metas.Add(new Meta()
        {
            Id = "BaseUrl",
            Value = serviceConfig.BaseUrl
        });
        
        metas.AddRange(req.Meta.Select(meta => new Meta {Id = meta.Key, Value = meta.Value}));


        var binder = new Binder()
        {
            Bind = bind,
            ApiUrl = req.ApiUrl,
            Meta = metas
        };

        await _binderRepository.AddAsync(binder, ct);
        await SendOkAsync(ct);
    }
}

internal class Request
{
    public Guid ServiceConfigId { get; set; }

    public string ApiUrl { get; set; }

    public string Bind { get; set; }

    public Dictionary<string, string> Meta { get; set; }
}

internal sealed class RequestValidator : Validator<Request>
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