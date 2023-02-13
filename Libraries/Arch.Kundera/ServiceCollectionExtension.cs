using Core.Library;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Kundera;

public static class ServiceCollectionExtension
{
    public static void AddKundera(this IServiceCollection services, IConfiguration configuration)
    {
        KunderaAuthorizationSettings.BaseUrl = configuration.GetSection("Kundera:BaseUrl").Value ??
                                               throw new Exception("Enter Kundera:BaseUrl in appsettings.json");
        services.AddArchMiddleware<KunderaAuthorizationMiddleware>();
        services.AddHttpClient("kundera", _ => { });
    }
}