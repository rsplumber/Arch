using Microsoft.Extensions.DependencyInjection;

namespace Arch.Configurations;

public sealed class ArchOptions
{
    public IServiceCollection Services { get; init; } = default!;
}