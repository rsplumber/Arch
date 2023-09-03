using Arch.Configurations;

namespace Arch.LoadBalancer.Configurations;

public static class ArchOptionsExtension
{
    public static void ConfigureLoadBalancer(this ArchOptions archOptions, Action<LoadBalancerOptions>? options = null) => options?.Invoke(new LoadBalancerOptions
    {
        Services = archOptions.Services
    });
}