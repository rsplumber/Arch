using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Core.Middlewares;

internal sealed class ResponseHandlerMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var requestState = context.ProcessorState<RequestState>();
        if (requestState.IgnoreDispatch())
        {
            await next(context);
            return;
        }

        if (requestState.ResponseInfo is null)
        {
            await context.Response.SendStringAsync(string.Empty, 400);
            await next(context);
            return;
        }

        context.Response.ContentType = requestState.ResponseInfo.ContentType;
        context.Response.StatusCode = requestState.ResponseInfo.Code;
        await context.Response.SendAsync(new
        {
            requestId = requestState.RequestInfo.RequestId,
            requestDateUtc = requestState.RequestInfo.RequestDateUtc,
            data = requestState.ResponseInfo.Value
        });
        await next(context);
    }
}