using Microsoft.Extensions.DependencyInjection;

namespace Arch.Data.Caching.Abstractions;

public sealed class CachingOptions
{
    public IServiceCollection Services { get; set; } = default!;
}