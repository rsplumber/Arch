using Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Logging.Logstash;

public static class ServiceCollectionExtension
{
    public static void AddLoggingLogstash(this IServiceCollection services)
    {
        services.AddScoped<IArchLogger, ArchLogger>();
    }
}