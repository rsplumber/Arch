using Microsoft.AspNetCore.Http;

namespace Core.Pipeline;

internal sealed class ExceptionHandlerMiddleware : IMiddleware
{
    private const string InternalServerErrorMessage = "Whoops :( , somthing impossibly went wrong!";
    private const string ContentType = "application/json; charset=utf-8";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var response = context.Response;
            response.ContentType = ContentType;
            string message;
            switch (exception)
            {
                case ArchException arch:
                    response.StatusCode = arch.Code;
                    message = arch.Message;
                    break;
                default:
                    response.StatusCode = 500;
                    message = InternalServerErrorMessage;
                    break;
            }

            await response.WriteAsync(message);
        }
    }
}