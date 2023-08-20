using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Core.Pipeline;

internal sealed class ExceptionHandlerMiddleware : IMiddleware
{
    private const int InternalServerErrorCode = 500;
    private const string InternalServerErrorMessage = "Whoops :( , somthing impossibly went wrong!";
    private const string ContentType = "application/json; charset=utf-8";

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            return next(context);
        }
        catch (Exception exception)
        {
            context.Response.ContentType = ContentType;
            if (exception is ArchException archException)
            {
                return context.Response.SendAsync(archException.Message, archException.Code);
            }

            return context.Response.SendAsync(InternalServerErrorMessage, InternalServerErrorCode);
        }
    }
}