using KunderaNet.Services.Authorization.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Kundera;

public static class ServiceCollectionExtension
{
    public static void AddKundera(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddKunderaHttpService(configuration);
        services.AddSingleton<KunderaAuthorizationMiddleware>();
    }
}