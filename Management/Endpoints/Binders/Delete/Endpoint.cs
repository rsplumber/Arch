using Core;
using Core.ServiceConfigs;
using FastEndpoints;
using FluentValidation;

namespace Management.Endpoints.Binders.Delete;

internal sealed class Endpoint : Endpoint<RemoveBinderRequest>
{
    private readonly IBinderRepository _binderRepository;


    public Endpoint(IBinderRepository binderRepository)
    {
        _binderRepository = binderRepository;
    }

    public override void Configure()
    {
        Delete("binder");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(RemoveBinderRequest req, CancellationToken ct)
    {
        var binder = await _binderRepository.FindAsync(req.Id, ct);
        if (binder is null)
        {
            throw new Exception("Binder not found");
        }

        BaseTree.BaseTreeNode.RemoveAsync(binder.Id, ct);
        await _binderRepository.DeleteAsync(binder, ct);
        await SendOkAsync(ct);
    }
}

internal sealed class EndpointSummary : Summary<Endpoint>
{
    public EndpointSummary()
    {
        Summary = "Delete Binder in the system";
        Description = "Delete Binder in the system";
        Response(200, "Binder was successfully Deleted");
    }
}

internal sealed class RequestValidator : Validator<RemoveBinderRequest>
{
    public RequestValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty().WithMessage("Enter Id")
            .NotNull().WithMessage("Enter Id");
    }
}