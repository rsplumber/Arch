using Core.Entities.EndpointDefinitions;
using Core.Entities.ServiceConfigs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Data.Sql;

public static class ServiceCollectionExtension
{
    public static void AddData(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(
            builder => builder.UseNpgsql(configuration.GetConnectionString("Default")));
        services.AddScoped<IServiceConfigRepository, ServiceConfigRepository>();
        services.AddScoped<IEndpointDefinitionRepository, EndpointDefinitionRepository>();
    }
}