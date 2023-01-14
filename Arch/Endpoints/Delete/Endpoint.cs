using Core;
using Core.HttpClientFactories;
using FastEndpoints;

namespace Arch.Endpoints.Delete;

internal sealed class Endpoint : EndpointWithoutRequest
{
    private readonly IHttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContext;

    public Endpoint(IHttpClient httpClient, IHttpContextAccessor httpContext)
    {
        _httpClient = httpClient;
        _httpContext = httpContext;
    }

    public override void Configure()
    {
        Delete();
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var requestInfo = new RequestInfo
        {
            Headers = _httpContext.HttpContext.Request.Headers.ToDictionary(a => a.Key, a => string.Join(";", a.Value)),
            Method = _httpContext.HttpContext.Request.Method,
            Path = _httpContext.HttpContext.Request.Path
        };
        var response = await _httpClient.SendAsync(requestInfo);
        await SendOkAsync(response, ct);
    }
}