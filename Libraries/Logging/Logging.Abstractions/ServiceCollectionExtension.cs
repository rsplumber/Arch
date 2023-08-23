using Microsoft.Extensions.DependencyInjection;

namespace Logging.Abstractions;

public static class ServiceCollectionExtension
{
    public static void AddLogging(this IServiceCollection services, Action<LoggingOptions>? options = null) => options?.Invoke(new LoggingOptions
    {
        Services = services
    });
}