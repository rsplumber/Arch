using Microsoft.Extensions.DependencyInjection;

namespace Core.Library;

public static class ServiceCollectionExtension
{
    public static void AddArchMiddleware<TMiddleware>(this IServiceCollection services)
        where TMiddleware : ArchMiddleware
    {
        services.AddSingleton<TMiddleware>();
    }
}