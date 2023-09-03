using Microsoft.Extensions.DependencyInjection;

namespace Arch.Core.Extensions;

public sealed class CoreOptions
{
    public IServiceCollection Services { get; init; } = default!;
}