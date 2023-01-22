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
        if (!dbContext.ServiceConfigs.Any(config => config.Name == "arch"))
        {
            var serviceConfig = new ServiceConfig
            {
                Name = "arch",
                BaseUrl = "http://localhost:5228"
            };
            dbContext.ServiceConfigs.Add(serviceConfig);
            dbContext.SaveChanges();
            if (dbContext.EndpointDefinitions.Any()) return;
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
            dbContext.ServiceConfigs.Update(serviceConfig);
            dbContext.SaveChanges();
        }
    }
}