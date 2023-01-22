using Core;
using Core.ServiceConfigs;
using FastEndpoints;
using FluentValidation;

namespace Management.Endpoints.Binders.Delete;

internal sealed class Endpoint : Endpoint<Request>
{
    private readonly IBinderRepository _binderRepository;


    public Endpoint(IBinderRepository binderRepository)
    {
        _binderRepository = binderRepository;
    }

    public override void Configure()
    {
        Delete("binders/{id}");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var binder = await _binderRepository.FindAsync(req.Id, ct);
        if (binder is null)
        {
            throw new Exception("Binder not found");
        }

        BaseTree.BaseTreeNode.RemoveAsync(binder.Bind, ct);
        await _binderRepository.DeleteAsync(binder, ct);
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