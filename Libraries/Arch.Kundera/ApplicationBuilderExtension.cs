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
        if (!dbContext.ServiceConfigs.Any(config => config.Name == "kundera"))
        {
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
                Endpoint = "identity/api/v1/authenticate",
                Pattern = "identity/api/v1/authenticate",
                MapTo = "api/v1/authenticate",
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
                Endpoint = "identity/api/v1/authenticate/refresh",
                Pattern = "identity/api/v1/authenticate/refresh",
                MapTo = "api/v1/authenticate/refresh",
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
        }

        var archServiceConfig = dbContext.ServiceConfigs
            .Include(config => config.Meta)
            .Include(config => config.EndpointDefinitions)
            .ThenInclude(definition => definition.Meta)
            .First(config => config.Name == "arch");

        if (archServiceConfig.Meta.All(meta => meta.Key != "service_secret"))
        {
            archServiceConfig.Meta.Add(new()
            {
                Key = "service_secret",
                Value = configuration.GetSection("Kundera:Arch_Service_Secret").Value ??
                        throw new Exception("Enter Kundera:Arch_Service_Secret in appsettings.json")
            });
        }

        if (archServiceConfig.EndpointDefinitions.All(definition => definition.Pattern != "gateway/api/v1/endpoint-definitions/##/security/permissions"))
        {
            archServiceConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "gateway/api/v1/endpoint-definitions/{id}/security/permissions",
                Pattern = "gateway/api/v1/endpoint-definitions/##/security/permissions",
                MapTo = "gateway/api/v1/endpoint-definitions{0}/security/permissions",
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
        }

        if (archServiceConfig.EndpointDefinitions.All(definition => definition.Pattern != "gateway/api/v1/endpoint-definitions/##/security/allow-anonymous"))
        {
            archServiceConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "gateway/api/v1/endpoint-definitions/{id}/security/allow-anonymous",
                Pattern = "gateway/api/v1/endpoint-definitions/##/security/allow-anonymous",
                MapTo = "gateway/api/v1/endpoint-definitions{0}/security/allow-anonymous",
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
        }


        dbContext.ServiceConfigs.Update(archServiceConfig);
        dbContext.SaveChanges();
    }
}