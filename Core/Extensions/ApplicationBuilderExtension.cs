using Arch.Configurations;
using Arch.Core.Pipeline;
using Microsoft.AspNetCore.Builder;

namespace Arch.Core.Extensions;

public static class ApplicationBuilderExtension
{
    public static void UseCore(this ArchExecutionOptions archExecution,
        Action<BeforeDispatchingOptions>? beforeDispatchingOptions,
        Action<AfterDispatchingOptions>? afterDispatchingOptions
    )
    {
        archExecution.ApplicationBuilder.UseMiddleware<ExceptionHandlerMiddleware>();
        archExecution.ApplicationBuilder.UseMiddleware<RequestExtractorMiddleware>();
        beforeDispatchingOptions?.Invoke(new BeforeDispatchingOptions
        {
            ApplicationBuilder = archExecution.ApplicationBuilder
        });
        archExecution.ApplicationBuilder.UseMiddleware<RequestDispatcherMiddleware>();
        afterDispatchingOptions?.Invoke(new AfterDispatchingOptions
        {
            ApplicationBuilder = archExecution.ApplicationBuilder
        });
        archExecution.ApplicationBuilder.UseMiddleware<LoggerMiddleware>();
        archExecution.ApplicationBuilder.UseMiddleware<ResponseHandlerMiddleware>();
    }
}