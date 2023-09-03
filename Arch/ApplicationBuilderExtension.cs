using Arch.Configurations;
using Arch.Core.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Arch;

public static class ApplicationBuilderExtension
{
    public static void UseArch(this IApplicationBuilder app,
        Action<ArchExecutionOptions> archOptions,
        Action<BeforeDispatchingOptions>? beforeDispatchingOptions,
        Action<AfterDispatchingOptions>? afterDispatchingOptions)
    {
        app.Use(async (context, next) =>
        {
            context.Request.EnableBuffering();
            await next();
        });

        var archExecutionOptions = new ArchExecutionOptions
        {
            ApplicationBuilder = app
        };
        archExecutionOptions.UseCore(beforeDispatchingOptions, afterDispatchingOptions);
        archOptions?.Invoke(archExecutionOptions);
    }
}