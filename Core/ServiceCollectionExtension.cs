using Core.Middlewares;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Core;

public static class ServiceCollectionExtension
{
    public static void AddCore(this IServiceCollection services, Action<IServiceCollection> libraries)
    {
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
    }
}