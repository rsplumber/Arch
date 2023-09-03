using Microsoft.Extensions.DependencyInjection;

namespace Arch.Logging.Abstractions;

public sealed class LoggingOptions
{
    public IServiceCollection Services { get; init; } = default!;
}