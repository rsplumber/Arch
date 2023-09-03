using Arch.Configurations;

namespace Arch.EndpointGraph.Abstractions;

public static class ArchExecutionOptionsExtension
{
    public static void UseEndpointGraph(this ArchExecutionOptions archExecutionOptions, Action<EndpointGraphExecutionOptions>? options) => options?.Invoke(new EndpointGraphExecutionOptions
    {
        ServiceProvider = archExecutionOptions.ApplicationBuilder.ApplicationServices
    });
}