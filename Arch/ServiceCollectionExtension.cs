using System.Text.Json;

namespace Arch;

public static class ServiceCollectionExtension
{
    public static void AddArch(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TesServicesOptions>(configuration.GetSection("Services"));
        services.AddHttpClientFactories(configuration);
    }

    private static void AddHttpClientFactories(this IServiceCollection services, IConfiguration configuration)
    {
        var tesServicesString = configuration.GetSection("Services").Value;
        var tesServices = JsonSerializer.Deserialize<List<Service>>(tesServicesString!);
        tesServices!.ForEach(service =>
        {
            services.AddHttpClient(service.Name, c =>
            {
                c.BaseAddress = new Uri(service.BaseUrl);
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            });
        });
    }
}