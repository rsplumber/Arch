using Arch.Configurations;

namespace Arch.Logging.Abstractions;

public static class ArchOptionsExtension
{
    public static void AddLogging(this ArchOptions archOptions, Action<LoggingOptions>? options = null) => options?.Invoke(new LoggingOptions
    {
        Services = archOptions.Services
    });
}