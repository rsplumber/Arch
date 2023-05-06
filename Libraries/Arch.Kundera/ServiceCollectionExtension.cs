using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Kundera;

public static class ServiceCollectionExtension
{
    public static void AddKundera(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<KunderaAuthorizationMiddleware>();
        services.AddHttpClient("kundera", client =>
        {
            client.BaseAddress = new Uri(configuration.GetSection("Kundera:BaseUrl").Value ??
                                         throw new Exception("Enter Kundera:BaseUrl in appsettings.json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });
    }
}