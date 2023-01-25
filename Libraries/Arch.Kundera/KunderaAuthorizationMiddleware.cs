using Microsoft.AspNetCore.Http;

namespace Arch.Kundera;

internal class KunderaAuthorizationMiddleware : IMiddleware
{
    private const string HttpClientFactoryKey = "kundera";
    private readonly IHttpClientFactory _clientFactory;
    private const string UserIdKey = "user_id";

    public KunderaAuthorizationMiddleware(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.Items[UserIdKey] = "dd7dd1a9-0db6-45b3-93c6-2e2026f2d050";
        await next(context);
    }
}