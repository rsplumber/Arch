using System.Text.Json;
using Core.Middlewares.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Core.Middlewares;

internal sealed class ResponseHandlerMiddleware : ArchMiddleware
{
    public override async Task HandleAsync(HttpContext context, RequestDelegate next)
    {
        if (EndpointDefinition is null || RequestInfo is null)
        {
            throw new InvalidRequestException();
        }

        if (IgnoreDispatch())
        {
            await next(context);
            return;
        }

        if (ResponseInfo is null)
        {
            throw new InvalidResponseException();
        }

        context.Response.ContentType = ResponseInfo.ContentType;
        context.Response.StatusCode = ResponseInfo.Code;

        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            requestId = RequestInfo.RequestId,
            requestDateUtc = RequestInfo.RequestDateUtc,
            data = ResponseInfo.Value
        }));
    }
}