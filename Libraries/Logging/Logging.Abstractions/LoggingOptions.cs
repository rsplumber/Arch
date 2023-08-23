using Microsoft.Extensions.DependencyInjection;

namespace Logging.Abstractions;

public sealed class LoggingOptions
{
    public IServiceCollection Services { get; init; } = default!;
}