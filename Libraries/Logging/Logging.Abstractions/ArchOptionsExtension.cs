using Arch.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Logging.Abstractions;

public static class ArchOptionsExtension
{
    public static void AddLogging(this ArchOptions archOptions, Action<LoggingOptions>? options = null)
    {
        archOptions.Services.AddSingleton<LoggerMiddleware>();
        archOptions.Services.AddTransient<ArchInternalLogEventHandler>();
        options?.Invoke(new LoggingOptions
        {
            Services = archOptions.Services
        });
    }
}