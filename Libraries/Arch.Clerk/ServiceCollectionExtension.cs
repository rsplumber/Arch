using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Clerk;

public static class ServiceCollectionExtension
{
    public static void AddClerkAccounting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<CheckAccountingMiddleware>();
        services.AddHttpClient("clerk", _ => { });
    }
}