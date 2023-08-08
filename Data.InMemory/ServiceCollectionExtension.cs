using Core.Containers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Data.InMemory;

public static class ServiceCollectionExtension
{
    public static void AddInMemoryDataContainers(this IServiceCollection services)
    {
        services.TryAddSingleton<IEndpointDefinitionContainer, InMemoryEndpointDefinitionContainer>();
        services.TryAddScoped<IContainerInitializer, InMemoryContainerInitializer>();
        services.AddScoped<EndpointDefinitionEventHandlers>();
    }
}