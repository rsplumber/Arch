using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Kundera;

public static class ServiceCollectionExtension
{
    public static void AddKundera(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<KunderaAuthorizationMiddleware>();
        services.AddHttpClient("kundera", _ => { });
    }
}