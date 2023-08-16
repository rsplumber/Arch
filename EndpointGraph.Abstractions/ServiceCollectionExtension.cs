using Microsoft.Extensions.DependencyInjection;

namespace EndpointGraph.Abstractions;

public static class ServiceCollectionExtension
{
    public static void AddEndpointGraph(this IServiceCollection services, Action<EndpointGraphOptions>? options = null) => options?.Invoke(new EndpointGraphOptions
    {
        Services = services
    });
}