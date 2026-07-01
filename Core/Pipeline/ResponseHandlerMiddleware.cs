using Arch.Core.Extensions.Http;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Arch.Core.Pipeline;

internal sealed class ResponseHandlerMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var state = context.RequestState();
        if (state.IgnoreDispatch())
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        if (state.HasEmptyResponse())
        {
            ApplyArchHeaders(context, state.RequestInfo.RequestId, null);
            await context.Response.SendStringAsync(string.Empty, 400)
                .ConfigureAwait(false);
            return;
        }

        var responseInfo = state.ResponseInfo!;

        foreach (var (key, value) in responseInfo.Headers)
            context.Response.Headers[key] = value;

        ApplyArchHeaders(context, state.RequestInfo.RequestId, responseInfo.ResponseTimeMilliseconds);

        await context.Response.SendAsync(new Response
        {
            RequestId = state.RequestInfo.RequestId,
            RequestDateUtc = state.RequestInfo.RequestDateUtc,
            Data = responseInfo.Value
        }, responseInfo.Code).ConfigureAwait(false);
    }

    private static void ApplyArchHeaders(HttpContext context, Guid requestId, long? responseTimeMs)
    {
        var headers = context.Response.Headers;

        // Standard headers
        headers["X-Request-Id"]          = requestId.ToString();
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"]        = "DENY";
        headers["Cache-Control"]          = "no-store";

        // Arch-specific identifying headers
        headers["X-Arch-Request-Id"]      = requestId.ToString();
        headers["X-Arch-Request-Date"]    = DateTime.UtcNow.ToString("o");
        headers["X-Arch-Powered-By"]      = "Arch";

        if (responseTimeMs.HasValue)
            headers["X-Arch-Response-Time-Ms"] = responseTimeMs.Value.ToString();
    }
}

public sealed record Response
{
    public required Guid RequestId { get; init; }

    public required DateTime RequestDateUtc { get; init; }

    public dynamic? Data { get; init; }
}