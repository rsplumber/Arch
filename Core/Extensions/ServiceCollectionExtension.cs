using Core.EndpointDefinitions;
using Core.EndpointDefinitions.Resolvers;
using Core.EndpointDefinitions.Services;
using Core.Pipeline;
using Core.ServiceConfigs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Core.Extensions;

public static class ServiceCollectionExtension
{
    public static void AddCore(this IServiceCollection services, Action<IServiceCollection> libraries)
    {
        services.AddSingleton<IServiceEndpointResolver, BasicServiceEndpointResolver>();
        services.TryAddSingleton<ExceptionHandlerMiddleware>();
        services.TryAddSingleton<RequestExtractorMiddleware>();
        libraries(services);
        services.TryAddSingleton<RequestDispatcherMiddleware>();
        services.TryAddSingleton<LoggerMiddleware>();
        services.TryAddSingleton<ResponseHandlerMiddleware>();
        services.TryAddTransient<ArchInternalLogEventHandler>();
        services.AddHttpClient("arch", client =>
        {
            client.DefaultRequestHeaders.Clear();
            client.Timeout = TimeSpan.FromSeconds(500.0);
            client.MaxResponseContentBufferSize = int.MaxValue;
        });
        services.AddScoped<IEndpointDefinitionResolver, EndpointDefinitionResolver>();
        services.AddScoped<IEndpointDefinitionService, EndpointDefinitionService>();
        services.AddScoped<IServiceConfigService, ServiceConfigService>();
    }
}