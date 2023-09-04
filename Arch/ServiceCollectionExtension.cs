using Arch.Configurations;
using Arch.Core.Extensions;
using Arch.Data.Abstractions;
using Arch.EndpointGraph.Abstractions;
using Arch.EventBus.Configurations;
using Arch.LoadBalancer.Configurations;
using FastEndpoints;
using Microsoft.Extensions.DependencyInjection;

namespace Arch;

public static class ServiceCollectionExtension
{
    public static void AddArch(this IServiceCollection services, Action<ArchOptions> archOptions)
    {
        var optionObject = new ArchOptions
        {
            Services = services
        };
        archOptions.Invoke(optionObject);

        services.AddCore();

        ArgumentNullException.ThrowIfNull(optionObject.DataOptions);
        optionObject.DataOptions.Invoke(new DataOptions
        {
            Services = services
        });

        ArgumentNullException.ThrowIfNull(optionObject.LoadBalancerOptions);
        optionObject.LoadBalancerOptions.Invoke(new LoadBalancerOptions
        {
            Services = services
        });

        ArgumentNullException.ThrowIfNull(optionObject.EventBusOptions);
        optionObject.EventBusOptions.Invoke(new EventBusOptions
        {
            Services = services
        });

        ArgumentNullException.ThrowIfNull(optionObject.EndpointGraphOptions);
        optionObject.EndpointGraphOptions.Invoke(new EndpointGraphOptions
        {
            Services = services
        });

        if (optionObject.HealthCheckEnabled)
        {
            services.AddHealthChecks();
        }

        if (optionObject.CorsEnabled)
        {
            services.AddCors();
        }

        services.AddFastEndpoints();
    }
}