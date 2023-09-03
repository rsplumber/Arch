using Arch.Configurations;

namespace Arch.EndpointGraph.Abstractions;

public static class ArchOptionsExtension
{
    public static void ConfigureEndpointGraph(this ArchOptions archOptions, Action<EndpointGraphOptions>? options = null) => options?.Invoke(new EndpointGraphOptions
    {
        Services = archOptions.Services
    });
}