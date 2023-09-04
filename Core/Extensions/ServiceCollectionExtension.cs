using Arch.Core.EndpointDefinitions;
using Arch.Core.EndpointDefinitions.Resolvers;
using Arch.Core.EndpointDefinitions.Services;
using Arch.Core.Pipeline;
using Arch.Core.ServiceConfigs.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Core.Extensions;

public static class ServiceCollectionExtension
{
    internal static void AddCore(this IServiceCollection services)
    {
        services.AddSingleton<ExceptionHandlerMiddleware>();
        services.AddSingleton<RequestExtractorMiddleware>();
        services.AddSingleton<RequestDispatcherMiddleware>();
        services.AddSingleton<ResponseHandlerMiddleware>();

        services.AddScoped<IEndpointDefinitionResolver, EndpointDefinitionResolver>();
        services.AddScoped<IEndpointDefinitionService, EndpointDefinitionService>();
        services.AddScoped<IServiceConfigService, ServiceConfigService>();
        services.AddHttpClient("arch", client =>
        {
            client.DefaultRequestHeaders.Clear();
            client.Timeout = TimeSpan.FromSeconds(500.0);
            client.MaxResponseContentBufferSize = int.MaxValue;
        });
    }
}