using System.Text.Json;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Core.Middlewares;

internal sealed class ResponseHandlerMiddleware : IMiddleware
{
    private const string IgnoreDispatchKey = "ignore_dispatch";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var requestState = context.ProcessorState<RequestState>();
        if (IgnoreDispatch())
        {
            await next(context);
            return;
        }

        if (requestState.ResponseInfo is null)
        {
            await context.Response.SendOkAsync();
            return;
        }

        context.Response.ContentType = requestState.ResponseInfo.ContentType;
        context.Response.StatusCode = requestState.ResponseInfo.Code;

        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            requestId = requestState.RequestInfo.RequestId,
            requestDateUtc = requestState.RequestInfo.RequestDateUtc,
            data = requestState.ResponseInfo.Value
        }));

        bool IgnoreDispatch() => requestState.EndpointDefinition.Meta.TryGetValue(IgnoreDispatchKey, out _);
    }
}