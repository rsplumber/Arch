using System.Text.Json;
using Core.Library.Exceptions;

namespace Application.Middlewares;

public class ExceptionHandlerMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";
            if (exception is ArchException arch)
            {
                response.StatusCode = arch.Code;
            }
            else
            {
                response.StatusCode = 500;
            }

            await response.WriteAsync(JsonSerializer.Serialize(new
            {
                message = exception.Message
            }));
        }
    }
}