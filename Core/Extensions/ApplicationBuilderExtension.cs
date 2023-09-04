using Arch.Core.Pipeline;
using Microsoft.AspNetCore.Builder;

namespace Arch.Core.Extensions;

public static class ApplicationBuilderExtension
{
    internal static void UseCore(this IApplicationBuilder app,
        Action<IApplicationBuilder>? beforeDispatchingOptions = null,
        Action<IApplicationBuilder>? afterDispatchingOptions = null
    )
    {
        app.UseMiddleware<ExceptionHandlerMiddleware>();
        app.UseMiddleware<RequestExtractorMiddleware>();
        beforeDispatchingOptions?.Invoke(app);
        app.UseMiddleware<RequestDispatcherMiddleware>();
        afterDispatchingOptions?.Invoke(app);
        app.UseMiddleware<ResponseHandlerMiddleware>();
    }
}