using Arch.EndpointGraph.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.EndpointGraph.InMemory;

public static class EndpointGraphExecutionOptionsExtension
{
    public static void UseInMemory(this EndpointGraphExecutionOptions endpointGraphExecutionOptions) { }

    public static void InitializeWith(this EndpointGraphExecutionOptions endpointGraphExecutionOptions, IEnumerable<string> endpoints)
    {
        using var scope = endpointGraphExecutionOptions.ServiceProvider
            .GetRequiredService<IServiceScopeFactory>()
            .CreateScope();

        var graph = scope.ServiceProvider.GetRequiredService<IEndpointGraph>();

        foreach (var endpoint in endpoints)
            graph.AddAsync(endpoint).GetAwaiter().GetResult();
    }
}
