using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Core.Pipeline;

internal sealed class ExceptionHandlerMiddleware : IMiddleware
{
    private const int InternalServerErrorCode = 500;
    private const string InternalServerErrorMessage = "Whoops :( , somthing impossibly went wrong!";
    private const string ContentType = "application/json; charset=utf-8";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            context.Response.ContentType = ContentType;
            if (exception is ArchException archException)
            {
                await context.Response.SendAsync(archException.Message, archException.Code)
                    .ConfigureAwait(false);
                return;
            }

            await context.Response.SendAsync(InternalServerErrorMessage, InternalServerErrorCode)
                .ConfigureAwait(false);
        }
    }
}