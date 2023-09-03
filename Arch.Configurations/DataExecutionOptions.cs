using Microsoft.AspNetCore.Builder;

namespace Arch.Configurations;

public sealed class ArchExecutionOptions
{
    public IApplicationBuilder ApplicationBuilder { get; init; } = default!;
}