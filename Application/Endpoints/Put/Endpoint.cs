using Core.RequestDispatcher;

namespace Application.Endpoints.Put;

internal sealed class Endpoint : ArchEndpoint
{
    private readonly IRequestDispatcher _requestDispatcher;

    public Endpoint(IRequestDispatcher requestDispatcher)
    {
        _requestDispatcher = requestDispatcher;
    }

    public override void Configure()
    {
        Put("/{path}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(object req, CancellationToken ct)
    {
        await base.HandleAsync(req, ct);
        var response = await _requestDispatcher.ExecuteAsync(RequestInfo);
        await SendOkAsync(response, ct);
    }
}