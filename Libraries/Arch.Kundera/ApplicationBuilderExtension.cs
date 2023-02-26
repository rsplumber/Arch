using Core.EndpointDefinitions;
using Core.Library;
using Core.Metas;
using Core.ServiceConfigs;
using Data.Sql;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Kundera;

public static class ApplicationBuilderExtension
{
    public static void UseKundera(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseArchMiddleware<KunderaAuthorizationMiddleware>();

        using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
        var dbContext = serviceScope!.ServiceProvider.GetRequiredService<AppDbContext>();
        var currentConfig = dbContext.ServiceConfigs.FirstOrDefault(config => config.Name == "kundera");
        if (currentConfig is not null)
        {
            dbContext.ServiceConfigs.Remove(currentConfig);
            dbContext.SaveChanges();
        }

        var kunderaServiceConfig = new ServiceConfig
        {
            Name = "kundera",
            Primary = true,
            BaseUrl = configuration.GetSection("Kundera:BaseUrl").Value ??
                      throw new Exception("Enter Kundera:BaseUrl in appsettings.json")
        };

        kunderaServiceConfig.Meta.Add(new()
        {
            Key = "service_secret",
            Value = configuration.GetSection("Kundera:Kundera_Service_Secret").Value ??
                    throw new Exception("Enter Kundera:Kundera_Service_Secret in appsettings.json")
        });

        dbContext.ServiceConfigs.Add(kunderaServiceConfig);
        dbContext.SaveChanges();

        var createdConfig = dbContext.ServiceConfigs
            .Include(config => config.EndpointDefinitions)
            .ThenInclude(definition => definition.Meta)
            .FirstAsync(config => config.Id == kunderaServiceConfig.Id).Result;

        createdConfig.EndpointDefinitions.Add(new EndpointDefinition
        {
            Endpoint = "api/v1/authenticate",
            Pattern = "api/v1/authenticate",
            Method = HttpRequestMethods.Post,
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
            Method = HttpRequestMethods.Post,
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

        var archServiceConfig = dbContext.ServiceConfigs
            .Include(config => config.EndpointDefinitions)
            .First(config => config.Name == "arch");
        archServiceConfig.Meta.Add(new()
        {
            Key = "service_secret",
            Value = configuration.GetSection("Kundera:Arch_Service_Secret").Value ??
                    throw new Exception("Enter Kundera:Arch_Service_Secret in appsettings.json")
        });
        archServiceConfig.EndpointDefinitions.Add(new EndpointDefinition
        {
            Endpoint = "endpoint-definitions/{id}/security/permissions",
            Pattern = "endpoint-definitions/##/security/permissions",
            Method = HttpRequestMethods.Post,
            Meta = new List<Meta>
            {
                new()
                {
                    Key = "permissions",
                    Value = "arch_endpoint_definition_add_permission"
                },
            }
        });

        archServiceConfig.EndpointDefinitions.Add(new EndpointDefinition
        {
            Endpoint = "endpoint-definitions/{id}/security/allow-anonymous",
            Pattern = "endpoint-definitions/##/security/allow-anonymous",
            Method = HttpRequestMethods.Post,
            Meta = new List<Meta>
            {
                new()
                {
                    Key = "permissions",
                    Value = "arch_endpoint_definition_allow_anonymous"
                },
            }
        });


        dbContext.ServiceConfigs.Update(archServiceConfig);
        dbContext.SaveChanges();
    }
}