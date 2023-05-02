using Core.Middlewares;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Core;

public static class ServiceCollectionExtension
{
    public static void AddCore(this IServiceCollection services , Action<IServiceCollection> libraries)
    {
        services.TryAddSingleton<RequestExtractorMiddleware>();
        libraries(services);
        services.TryAddSingleton<RequestDispatcherMiddleware>();
        services.TryAddSingleton<ResponseHandlerMiddleware>();
    }
}