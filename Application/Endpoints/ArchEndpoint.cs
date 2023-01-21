using Core;
using FastEndpoints;

namespace Application.Endpoints;

public abstract class ArchEndpoint : Endpoint<object>
{
    public override Task HandleAsync(object req, CancellationToken ct)
    {
        RequestInfo = new()
        {
            Headers = HttpContext.Request.Headers.ToDictionary(a => a.Key, a => string.Join(";", a.Value)),
            Method = ExtractMethod(HttpContext.Request.Method),
            Path = HttpContext.Request.Path.Value!.Replace("%2F", "/"),
            Body = req
        };
        return Task.CompletedTask;
    }

    protected RequestInfo RequestInfo { get; private set; } = default!;

    private static HttpRequestMethod ExtractMethod(string method) => Enum.Parse<HttpRequestMethod>(method);
}