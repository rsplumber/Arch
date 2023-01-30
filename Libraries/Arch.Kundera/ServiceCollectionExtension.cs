using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Kundera;

public static class ServiceCollectionExtension
{
    public static void AddKundera(this IServiceCollection services, IConfiguration configuration)
    {
        var baseUrl = configuration.GetSection("Kundera:BaseUrl").Value;
        if (baseUrl is null)
        {
            throw new ArgumentException("Enter Kundera:BaseUrl in appsettings.json");
        }

        KunderaAuthorizationSettings.BaseUrl = baseUrl;
        services.AddSingleton<KunderaAuthorizationMiddleware>();
        services.AddHttpClient("kundera", _ => { });
    }
}