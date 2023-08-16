using Microsoft.Extensions.DependencyInjection;

namespace Caching.Abstractions;

public sealed class CachingOptions
{
    public IServiceCollection Services { get; set; } = default!;
}