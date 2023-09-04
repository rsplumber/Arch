using Arch.EndpointGraph.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.EndpointGraph.InMemory;

public static class EndpointGraphExecutionOptionsExtension
{
    public static void UseInMemory(this EndpointGraphExecutionOptions endpointGraphExecutionOptions)
    {
    }

    public static void InitializeWith(this EndpointGraphExecutionOptions endpointGraphExecutionOptions, IEnumerable<string> endpoints)
    {
        using var serviceScope = endpointGraphExecutionOptions.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var endpointPatternTree = serviceScope.ServiceProvider.GetRequiredService<IEndpointGraph>();
        foreach (var definition in endpoints)
        {
            endpointPatternTree.AddAsync(definition).GetAwaiter().GetResult();
        }
    }
}