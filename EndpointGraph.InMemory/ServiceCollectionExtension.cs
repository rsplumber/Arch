using Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EndpointGraph.InMemory;

public static class ServiceCollectionExtension
{
    public static void AddInMemoryEndpointGraph(this IServiceCollection services)
    {
        services.TryAddSingleton<IEndpointGraph, InMemoryEndpointGraph>();
    }
}