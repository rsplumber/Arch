using Arch.Core;
using Arch.LoadBalancer.Configurations;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arch.LoadBalancer.Basic;

public static class LoadBalancerOptionsExtension
{
    public static void UseBasic(this LoadBalancerOptions options)
    {
        options.Services.TryAddSingleton<IServiceEndpointResolver, BasicServiceEndpointResolver>();
    }
}