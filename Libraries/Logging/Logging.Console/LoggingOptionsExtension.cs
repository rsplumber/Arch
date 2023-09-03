using Arch.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Logging.Console;

public static class LoggingOptionsExtension
{
    public static void UseConsole(this LoggingOptions options)
    {
        options.Services.AddSingleton<IArchLogger, ArchLogger>();
    }
}