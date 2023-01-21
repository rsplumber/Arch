using Core.RequestDispatcher;

namespace Application.Endpoints.Patch;

internal sealed class Endpoint : ArchEndpoint
{
    private readonly IRequestDispatcher _requestDispatcher;

    public Endpoint(IRequestDispatcher requestDispatcher)
    {
        _requestDispatcher = requestDispatcher;
    }

    public override void Configure()
    {
        Patch("/{path}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(object req, CancellationToken ct)
    {
        await base.HandleAsync(req, ct);
        var response = await _requestDispatcher.ExecuteAsync(RequestInfo);
        await SendOkAsync(response, ct);
    }
}