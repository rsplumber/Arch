namespace EndpointGraph.Abstractions;

public static class ServiceProviderExtension
{
    public static void UseEndpointGraph(this IServiceProvider serviceProvider, Action<EndpointGraphExecutionOptions>? options) => options?.Invoke(new EndpointGraphExecutionOptions
    {
        ServiceProvider = serviceProvider
    });
}