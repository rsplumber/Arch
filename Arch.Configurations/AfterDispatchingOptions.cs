using Microsoft.AspNetCore.Builder;

namespace Arch.Configurations;

public class AfterDispatchingOptions
{
    public IApplicationBuilder ApplicationBuilder { get; init; } = default!;
}