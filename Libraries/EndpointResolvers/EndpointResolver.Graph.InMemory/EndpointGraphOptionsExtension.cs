using Arch.Core.EndpointResolver;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arch.EndpointResolver.Graph.InMemory;

public static class EndpointGraphOptionsExtension
{
    public static void UseInMemory(this EndpointGraphOptions options)
    {
        options.Services.TryAddSingleton<IEndpointGraph, InMemoryEndpointGraph>();
    }
}