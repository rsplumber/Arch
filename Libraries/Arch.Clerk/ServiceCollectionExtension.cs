using System.Net;
using System.Net.Http.Headers;
using Core.Library;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Clerk;

public static class ServiceCollectionExtension
{
    public static void AddClerkAccounting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddArchMiddleware<CheckAccountingMiddleware>();
        services.AddHttpClient("clerk", client =>
        {
            client.DefaultRequestVersion = HttpVersion.Version20;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
            client.BaseAddress = new Uri(configuration.GetSection("Clerk:BaseUrl").Value ??
                                         throw new Exception("Enter Clerk:BaseUrl in appsettings.json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });
    }
}