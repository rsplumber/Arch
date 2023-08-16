using EndpointGraph.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EndpointGraph.InMemory;

public static class EndpointGraphOptionsExtension
{
    public static void UseInMemoryEndpointGraph(this EndpointGraphOptions options)
    {
        options.Services.TryAddSingleton<IEndpointGraph, InMemoryEndpointGraph>();
    }
}