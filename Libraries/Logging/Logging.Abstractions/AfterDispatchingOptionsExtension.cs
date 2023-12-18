using Arch.Configurations;
using Microsoft.AspNetCore.Builder;

namespace Arch.Logging.Abstractions;

public static class AfterDispatchingOptionsExtension
{
    public static void UseLogging(this AfterDispatchingOptions afterDispatchingOptions)
    {
        afterDispatchingOptions.ApplicationBuilder.UseMiddleware<LoggerMiddleware>();
    }

    public static void UseLogging(this AfterDispatchingOptions afterDispatchingOptions, Action<LoggingExecutionOptions>? options)
    {
        afterDispatchingOptions.ApplicationBuilder.UseMiddleware<LoggerMiddleware>();
        options?.Invoke(new LoggingExecutionOptions
        {
            ApplicationBuilder = afterDispatchingOptions.ApplicationBuilder
        });
    }
}