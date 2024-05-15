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
            await context.Response.SendStringAsync(string.Empty, 400)
                .ConfigureAwait(false);
            return;
        }

        await context.Response.SendAsync(new Response
        {
            RequestId = state.RequestInfo.RequestId,
            RequestDateUtc = state.RequestInfo.RequestDateUtc,
            Data = state.ResponseInfo!.Value
        }, state.ResponseInfo.Code).ConfigureAwait(false);
    }
}

public sealed record Response
{
    public required Guid RequestId { get; init; }

    public required DateTime RequestDateUtc { get; init; }

    public dynamic? Data { get; init; }
}