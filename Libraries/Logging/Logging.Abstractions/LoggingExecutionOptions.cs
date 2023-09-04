using Microsoft.AspNetCore.Builder;

namespace Arch.Logging.Abstractions;

public sealed class LoggingExecutionOptions
{
    public IApplicationBuilder ApplicationBuilder { get; init; } = default!;
}