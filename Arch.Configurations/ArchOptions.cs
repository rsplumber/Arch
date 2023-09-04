using Arch.Data.Abstractions;
using Arch.EndpointGraph.Abstractions;
using Arch.LoadBalancer.Configurations;
using EventBus.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Configurations;

public sealed class ArchOptions
{
    public IServiceCollection Services { get; init; } = default!;

    internal Action<LoadBalancerOptions> LoadBalancerOptions { get; private set; } = default!;

    internal Action<EventBusOptions> EventBusOptions { get; private set; } = default!;

    internal Action<EndpointGraphOptions> EndpointGraphOptions { get; private set; } = default!;

    internal Action<DataOptions> DataOptions { get; private set; } = default!;

    internal bool HealthCheckEnabled { get; private set; }

    internal bool CorsEnabled { get; private set; }

    public void EnableHealthCheck() => HealthCheckEnabled = true;

    public void EnableCors() => CorsEnabled = true;

    public void ConfigureLoadBalancer(Action<LoadBalancerOptions> loadBalancerOptions) => LoadBalancerOptions = loadBalancerOptions;

    public void ConfigureEventBus(Action<EventBusOptions> eventBusOptions) => EventBusOptions = eventBusOptions;

    public void ConfigureEndpointGraph(Action<EndpointGraphOptions> endpointGraphOptions) => EndpointGraphOptions = endpointGraphOptions;

    public void ConfigureData(Action<DataOptions> dataOptions) => DataOptions = dataOptions;
}