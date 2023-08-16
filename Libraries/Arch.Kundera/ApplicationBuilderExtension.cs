using Core.EndpointDefinitions;
using Core.Metas;
using Core.ServiceConfigs;
using Core.ServiceConfigs.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Kundera;

public static class ApplicationBuilderExtension
{
    public static void UseKundera(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseMiddleware<KunderaAuthorizationMiddleware>();

        using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
        var serviceConfigRepository = serviceScope!.ServiceProvider.GetRequiredService<IServiceConfigRepository>();

        if (serviceConfigRepository.FindByNameAsync("kundera").Result is not null) return;

        var kunderaServiceConfig = ServiceConfig.CreatePrimary("kundera", configuration.GetSection("Kundera:BaseUrl").Value ??
                                                                          throw new Exception("Enter Kundera:BaseUrl in appsettings.json"));

        kunderaServiceConfig.Add(new Meta
        {
            Key = "service_secret",
            Value = configuration.GetSection("Kundera:Kundera_Service_Secret").Value ??
                    throw new Exception("Enter Kundera:Kundera_Service_Secret in appsettings.json")
        });

        kunderaServiceConfig.EndpointDefinitions.Add(new EndpointDefinition
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
        kunderaServiceConfig.EndpointDefinitions.Add(new EndpointDefinition
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

        var archServiceConfig = serviceConfigRepository.FindByNameAsync("arch").Result;
        if (archServiceConfig is null) throw new ServiceConfigNotFoundException();
        var secretMeta = archServiceConfig.Meta.Find(meta => meta.Key == "service_secret");
        if (secretMeta is null)
        {
            archServiceConfig.Add(new Meta
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

        serviceConfigRepository.AddAsync(kunderaServiceConfig).Wait();
        serviceConfigRepository.UpdateAsync(archServiceConfig).Wait();
    }
}