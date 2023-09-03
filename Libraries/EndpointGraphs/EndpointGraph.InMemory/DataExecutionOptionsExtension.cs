using Arch.Core.ServiceConfigs;
using Arch.EndpointGraph.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.EndpointGraph.InMemory;

public static class DataExecutionOptionsExtension
{
    public static void UseInMemory(this EndpointGraphExecutionOptions endpointGraphExecutionOptions)
    {
        using var serviceScope = endpointGraphExecutionOptions.ServiceProvider.GetRequiredService<IServiceScopeFactory>()?.CreateScope();
        if (serviceScope is null) throw new ArgumentNullException(nameof(serviceScope), "Cannot create scope");

        var serviceConfigRepository = serviceScope.ServiceProvider.GetRequiredService<IServiceConfigRepository>();
        var endpointPatternTree = serviceScope.ServiceProvider.GetRequiredService<IEndpointGraph>();
        var serviceConfigs = serviceConfigRepository.FindAsync().Result;
        foreach (var config in serviceConfigs)
        {
            foreach (var definition in config.EndpointDefinitions)
            {
                definition.Meta.AddRange(config.Meta);
                endpointPatternTree.AddAsync(definition.Endpoint).GetAwaiter().GetResult();
            }
        }
    }
}