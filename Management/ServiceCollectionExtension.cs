using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Management;

public static class ServiceCollectionExtension
{
    public static void AddManagement(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ManagementDbContext>(
            builder => builder.UseNpgsql(configuration.GetConnectionString("Default")));
    }
}