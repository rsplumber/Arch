using Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Logging.Logstash;

public static class LoggingOptionsExtension
{
    public static void UseLogstash(this LoggingOptions options)
    {
        options.Services.AddScoped<IArchLogger, ArchLogger>();
    }
}