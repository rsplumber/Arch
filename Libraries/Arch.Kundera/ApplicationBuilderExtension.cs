using Core.EndpointDefinitions;
using Core.Metas;
using Core.ServiceConfigs;
using Data.Sql;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Kundera;

public static class ApplicationBuilderExtension
{
    public static void UseKundera(this IApplicationBuilder app)
    {
        app.UseMiddleware<KunderaAuthorizationMiddleware>();

        using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
        var context = serviceScope!.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.Migrate();

        //Seed internal APIs 
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (!dbContext.ServiceConfigs.Any(config => config.Name == "kundera"))
        {
            var serviceConfig = new ServiceConfig
            {
                Name = "kundera"
            };
            serviceConfig.Meta.Add(new()
            {
                Key = "base_url",
                Value = "http://localhost:5179"
            });

            dbContext.ServiceConfigs.Add(serviceConfig);
            dbContext.SaveChanges();

            var createdConfig = dbContext.ServiceConfigs
                .Include(config => config.EndpointDefinitions)
                .ThenInclude(definition => definition.Meta)
                .FirstAsync(config => config.Id == serviceConfig.Id).Result;
            createdConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "api/v1/authenticate",
                Pattern = "api/v1/authenticate",
                Method = "post",
                Meta = new List<Meta>
                {
                    new()
                    {
                        Key = "allow_anonymous",
                        Value = "true"
                    }
                }
            });
            createdConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "api/v1/authenticate/refresh",
                Pattern = "api/v1/authenticate/refresh",
                Method = "post",
                Meta = new List<Meta>
                {
                    new()
                    {
                        Key = "allow_anonymous",
                        Value = "true"
                    }
                }
            });
            dbContext.ServiceConfigs.Update(createdConfig);
            dbContext.SaveChanges();
        }
    }
}