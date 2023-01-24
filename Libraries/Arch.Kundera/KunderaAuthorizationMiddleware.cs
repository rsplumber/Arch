using Microsoft.AspNetCore.Http;

namespace Arch.Kundera;

internal class KunderaAuthorizationMiddleware : IMiddleware
{
    private const string HttpClientFactoryKey = "kundera";
    private readonly IHttpClientFactory _clientFactory;

    public KunderaAuthorizationMiddleware(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var client = _clientFactory.CreateClient(HttpClientFactoryKey);
        await next(context);
    }
}