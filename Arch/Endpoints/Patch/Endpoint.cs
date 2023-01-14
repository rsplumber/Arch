using Core;
using Core.HttpClientFactories;
using FastEndpoints;

namespace Arch.Endpoints.Patch;

internal sealed class Endpoint : Endpoint<object>
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
        Patch();
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(object request, CancellationToken ct)
    {
        var requestInfo = new RequestInfo
        {
            Body = request,
            Headers = _httpContext.HttpContext.Request.Headers.ToDictionary(a => a.Key, a => string.Join(";", a.Value)),
            Method = _httpContext.HttpContext.Request.Method,
            Path = _httpContext.HttpContext.Request.Path
        };
        var response = await _httpClient.SendAsync(requestInfo);
        await SendOkAsync(response, ct);
    }
}