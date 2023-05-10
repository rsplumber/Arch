using System.Text.Json;
using Core.Middlewares.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Core.Middlewares;

internal sealed class ResponseHandlerMiddleware : ArchMiddleware
{
    private const string ContentType = "application/json; charset=utf-8";

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

        context.Response.ContentType = ContentType;
        context.Response.StatusCode = ResponseInfo.Code;
        dynamic? data;
        try
        {
            data = ResponseInfo.Value is not null ? JsonSerializer.Deserialize<dynamic>(ResponseInfo.Value) : null;
        }
        catch (Exception e)
        {
            data = ResponseInfo.Value;
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            requestId = RequestInfo.RequestId,
            requestDateUtc = RequestInfo.RequestDateUtc,
            data
        }));
    }
}