using Microsoft.Extensions.DependencyInjection;

namespace Data.Abstractions;

public static class ServiceCollectionExtension
{
    public static void AddData(this IServiceCollection services, Action<DataOptions>? options = null) => options?.Invoke(new DataOptions
    {
        Services = services
    });
}