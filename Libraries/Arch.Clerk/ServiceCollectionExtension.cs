using Core.Library;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Clerk;

public static class ServiceCollectionExtension
{
    public static void AddClerkAccounting(this IServiceCollection services, IConfiguration configuration)
    {
        ClerkAccountingSettings.BaseUrl = configuration.GetSection("Clerk:BaseUrl").Value ??
                                          throw new Exception("Enter Clerk:BaseUrl in appsettings.json");
        services.AddArchMiddleware<CheckAccountingMiddleware>();
        services.AddHttpClient("clerk", _ => { });
    }
}