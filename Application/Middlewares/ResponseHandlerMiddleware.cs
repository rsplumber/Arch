using Core.Library;

namespace Application.Middlewares;

internal sealed class ResponseHandlerMiddleware : ArchMiddleware
{
    private const string ContentType = "application/json; charset=utf-8";

    public override async Task HandleAsync(HttpContext context, RequestDelegate next)
    {
        if (EndpointDefinition is null) return;

        if (IgnoreDispatch())
        {
            await next(context);
            return;
        }

        context.Response.ContentType = ContentType;
        context.Response.StatusCode = ResponseInfo!.Code;
        await context.Response.WriteAsync(ResponseInfo.Value);
    }
}