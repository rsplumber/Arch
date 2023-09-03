using Arch.EndpointGraph.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arch.EndpointGraph.InMemory;

public static class EndpointGraphOptionsExtension
{
    public static void UseInMemory(this EndpointGraphOptions options)
    {
        options.Services.TryAddSingleton<IEndpointGraph, InMemoryEndpointGraph>();
    }
}