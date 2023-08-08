using Core.EndpointDefinitions;
using Core.Metas;
using Core.ServiceConfigs;
using Data.EFCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Kundera;

public static class ApplicationBuilderExtension
{
    public static void UseKundera(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseMiddleware<KunderaAuthorizationMiddleware>();

        using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
        var dbContext = serviceScope!.ServiceProvider.GetRequiredService<AppDbContext>();
        var currentConfig = dbContext.ServiceConfigs
            .Include(config => config.EndpointDefinitions)
            .ThenInclude(definition => definition.Meta)
            .Include(config => config.Meta)
            .FirstOrDefault(config => config.Name == "kundera");
        if (currentConfig is not null)
        {
            dbContext.Metas.RemoveRange(currentConfig.Meta);
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
            .First(config => config.Id == kunderaServiceConfig.Id);

        createdConfig.EndpointDefinitions.Add(new EndpointDefinition
        {
            Endpoint = "api/v1/authenticate",
            Pattern = "api/v1/authenticate",
            MapTo = "api/v1/authenticate",
            Method = HttpMethod.Post,
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
            MapTo = "api/v1/authenticate/refresh",
            Method = HttpMethod.Post,
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
            .Include(config => config.Meta)
            .Include(config => config.EndpointDefinitions)
            .ThenInclude(definition => definition.Meta)
            .First(config => config.Name == "arch");

        var secretMeta = archServiceConfig.Meta.Find(meta => meta.Key == "service_secret");
        if (secretMeta is not null)
        {
            archServiceConfig.Meta.Remove(secretMeta);
        }

        archServiceConfig.Meta.Add(new()
        {
            Key = "service_secret",
            Value = configuration.GetSection("Kundera:Arch_Service_Secret").Value ??
                    throw new Exception("Enter Kundera:Arch_Service_Secret in appsettings.json")
        });
        if (archServiceConfig.EndpointDefinitions.All(definition => definition.Pattern != "gateway/api/v1/endpoint-definitions/##/security/permissions"))
        {
            archServiceConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "gateway/api/v1/endpoint-definitions/{id}/security/permissions",
                Pattern = "gateway/api/v1/endpoint-definitions/##/security/permissions",
                MapTo = "gateway/api/v1/endpoint-definitions/{0}/security/permissions",
                Method = HttpMethod.Post,
                Meta = new List<Meta>
                {
                    new()
                    {
                        Key = "permissions",
                        Value = "endpoint_definition_add_permission"
                    },
                }
            });
        }

        if (archServiceConfig.EndpointDefinitions.All(definition => definition.Pattern != "gateway/api/v1/endpoint-definitions/##/security/allow-anonymous"))
        {
            archServiceConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "gateway/api/v1/endpoint-definitions/{id}/security/allow-anonymous",
                Pattern = "gateway/api/v1/endpoint-definitions/##/security/allow-anonymous",
                MapTo = "gateway/api/v1/endpoint-definitions/{0}/security/allow-anonymous",
                Method = HttpMethod.Post,
                Meta = new List<Meta>
                {
                    new()
                    {
                        Key = "permissions",
                        Value = "endpoint_definition_allow_anonymous"
                    },
                }
            });
        }


        dbContext.ServiceConfigs.Update(archServiceConfig);
        dbContext.SaveChanges();
    }
}