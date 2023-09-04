using Arch.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Logging.Logstash;

public static class LoggingOptionsExtension
{
    public static void UseLogstash(this LoggingOptions options)
    {
        options.Services.AddSingleton<IArchLogger, ArchLogger>();
    }
}