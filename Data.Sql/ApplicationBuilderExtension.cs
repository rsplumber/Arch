using Core.Domains;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Data.Sql;

public static class ApplicationBuilderExtension
{
    public static void UseData(this IApplicationBuilder app)
    {
        using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope())
        {
            if (serviceScope == null) return;
            try
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.Migrate();
                SeedData(context);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    private static void SeedData(AppDbContext dbContext)
    {
        var serviceConfig = new ServiceConfig
        {
            Name = "arch",
            BaseUrl = "http://localhost:5228"
        };
        dbContext.ServiceConfigs.Add(serviceConfig);
        var result = dbContext.SaveChangesAsync().Result;
        serviceConfig.EndpointDefinitions.Add(new EndpointDefinition
        {
            Endpoint = "api/endpoint-definitions",
            Pattern = "api/endpoint-definitions"
        });
        serviceConfig.EndpointDefinitions.Add(new EndpointDefinition
        {
            Endpoint = "api/endpoint-definitions/{id}",
            Pattern = "api/endpoint-definitions/##"
        });
        serviceConfig.EndpointDefinitions.Add(new EndpointDefinition
        {
            Endpoint = "api/service-configs",
            Pattern = "api/service-configs"
        });
        serviceConfig.EndpointDefinitions.Add(new EndpointDefinition
        {
            Endpoint = "api/service-configs/{id}",
            Pattern = "api/service-configs/##"
        });
    }
}