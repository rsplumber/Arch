using Microsoft.AspNetCore.Builder;

namespace Arch.Configurations;

public class BeforeDispatchingOptions
{
    public IApplicationBuilder ApplicationBuilder { get; init; } = default!;
}