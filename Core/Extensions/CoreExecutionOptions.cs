using Microsoft.AspNetCore.Builder;

namespace Arch.Core.Extensions;

public sealed class CoreExecutionOptions
{
    public IApplicationBuilder ApplicationBuilder { get; init; } = default!;
}