using Core.EndpointDefinitions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Data.InMemory;

public static class ServiceCollectionExtension
{
    public static void AddInMemoryDataContainers(this IServiceCollection services)
    {
        services.TryAddSingleton<IEndpointDefinitionContainer, InMemoryEndpointDefinitionContainer>();
        services.TryAddScoped<InMemoryContainerInitializer>();
        services.AddScoped<EndpointDefinitionEventHandlers>();
    }
}