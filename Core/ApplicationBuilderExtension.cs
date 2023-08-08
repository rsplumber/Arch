using Core.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace Core;

public static class ApplicationBuilderExtension
{
    public static void UseCore(this IApplicationBuilder app,Action<IApplicationBuilder> beforeDispatch, Action<IApplicationBuilder> afterDispatch)
    {
        app.UseMiddleware<ExceptionHandlerMiddleware>();
        app.UseMiddleware<RequestExtractorMiddleware>();
        beforeDispatch(app);
        app.UseMiddleware<RequestDispatcherMiddleware>();
        afterDispatch(app);
        app.UseMiddleware<ResponseHandlerMiddleware>();
        app.UseMiddleware<LoggerMiddleware>();
    }
}