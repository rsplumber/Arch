using Arch.Configurations;
using Arch.Core.EndpointDefinitions;
using Arch.Core.EndpointDefinitions.Resolvers;
using Arch.Core.EndpointDefinitions.Services;
using Arch.Core.Pipeline;
using Arch.Core.ServiceConfigs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arch.Core.Extensions;

public static class ArchOptionsExtension
{
    internal static void AddCore(this ArchOptions archOptions)
    {
        archOptions.Services.TryAddSingleton<ExceptionHandlerMiddleware>();
        archOptions.Services.TryAddSingleton<RequestExtractorMiddleware>();
        archOptions.Services.TryAddSingleton<RequestDispatcherMiddleware>();
        archOptions.Services.TryAddSingleton<LoggerMiddleware>();
        archOptions.Services.TryAddSingleton<ResponseHandlerMiddleware>();
        archOptions.Services.TryAddTransient<ArchInternalLogEventHandler>();
        archOptions.Services.AddScoped<IEndpointDefinitionResolver, EndpointDefinitionResolver>();
        archOptions.Services.AddScoped<IEndpointDefinitionService, EndpointDefinitionService>();
        archOptions.Services.AddScoped<IServiceConfigService, ServiceConfigService>();
        archOptions.Services.AddHttpClient("arch", client =>
        {
            client.DefaultRequestHeaders.Clear();
            client.Timeout = TimeSpan.FromSeconds(500.0);
            client.MaxResponseContentBufferSize = int.MaxValue;
        });
    }
}