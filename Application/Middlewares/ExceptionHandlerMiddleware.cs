using System.Text.Json;
using Core;
using FluentValidation;

namespace Application.Middlewares;

internal sealed class ExceptionHandlerMiddleware : ArchMiddleware
{
    private const string InternalServerErrorMessage = "Whoops :( , somthing impossibly went wrong!";
    private const string ContentType = "application/json; charset=utf-8";

    public override async Task HandleAsync(HttpContext context, RequestDelegate next)
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
                case ValidationException validationException:
                    response.StatusCode = 400;
                    message = string.Join(", ", validationException.Errors
                        .DistinctBy(failure => failure.PropertyName)
                        .Select(failure => $"{failure.PropertyName} : {failure.ErrorMessage}"));
                    break;
                default:
                    response.StatusCode = 500;
                    message = InternalServerErrorMessage;
                    break;
            }

            await response.WriteAsync(JsonSerializer.Serialize(new
            {
                RequestInfo!.RequestId,
                RequestInfo!.RequestDateUtc,
                message
            }));
        }
    }
}