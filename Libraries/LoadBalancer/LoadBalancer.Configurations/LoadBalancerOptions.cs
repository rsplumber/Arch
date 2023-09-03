using Microsoft.Extensions.DependencyInjection;

namespace Arch.LoadBalancer.Configurations;

public sealed class LoadBalancerOptions
{
    public IServiceCollection Services { get; init; } = default!;
}