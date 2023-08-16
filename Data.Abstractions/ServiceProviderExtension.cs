namespace Data.Abstractions;

public static class ServiceProviderExtension
{
    public static void UseData(this IServiceProvider serviceProvider, Action<DataExecutionOptions>? options) => options?.Invoke(new DataExecutionOptions
    {
        ServiceProvider = serviceProvider
    });
}