using Core.ServiceConfigs;
using FastEndpoints;
using FluentValidation;

namespace Management.Endpoints.Binders.Update;

internal sealed class Endpoint : Endpoint<Request>
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

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var binder = await _binderRepository.FindAsync(req.Id, ct);
        var serviceConfig = await _serviceConfigRepository.FindByBinderAsync(binder.Id, ct);

        if (serviceConfig is null)
        {
            throw new Exception("ServiceConfig not found");
        }

        binder.Meta.Clear();

        binder.Meta.Add(new Meta()
        {
            Id = "BaseUrl",
            Value = serviceConfig.BaseUrl
        });

        foreach (var meta in req.Metas)
        {
            binder.Meta.Add(new Meta
            {
                Id = meta.Key,
                Value = meta.Value
            });
        }


        await SendOkAsync(ct);
    }
}

internal class Request
{
    public Guid Id { get; set; }

    public Dictionary<string, string> Metas { get; set; }
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