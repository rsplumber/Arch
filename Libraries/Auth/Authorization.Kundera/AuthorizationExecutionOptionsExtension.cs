﻿using Arch.Authorization.Abstractions;
using Arch.Core.ServiceConfigs;
using Arch.Core.ServiceConfigs.EndpointDefinitions;
using Arch.Core.ServiceConfigs.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Authorization.Kundera;

public static class AuthorizationExecutionOptionsExtension
{
    public static void UseKundera(this AuthorizationExecutionOptions executionOptions, IConfiguration configuration)
    {
        executionOptions.ApplicationBuilder.UseMiddleware<KunderaAuthorizationMiddleware>();

        using var serviceScope = executionOptions.ApplicationBuilder.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
        var serviceConfigRepository = serviceScope!.ServiceProvider.GetRequiredService<IServiceConfigRepository>();

        if (serviceConfigRepository.FindByNameAsync("kundera").Result is not null) return;

        var kunderaServiceConfig = ServiceConfig.CreatePrimary("kundera", configuration.GetSection("Kundera:BaseUrl").Value ??
                                                                          throw new Exception("Enter Kundera:BaseUrl in appsettings.json"));

        kunderaServiceConfig.AddMeta("service_secret", configuration.GetSection("Kundera:Kundera_Service_Secret").Value ??
                                                       throw new Exception("Enter Kundera:Kundera_Service_Secret in appsettings.json"));

        kunderaServiceConfig.EndpointDefinitions.Add(new EndpointDefinition
        {
            Endpoint = "api/v1/authenticate",
            Pattern = "api/v1/authenticate",
            MapTo = "api/v1/authenticate",
            Method = HttpMethod.Post,
            Meta = new Dictionary<string, string>
            {
                { "allow_anonymous", "true" }
            }
        });
        kunderaServiceConfig.EndpointDefinitions.Add(new EndpointDefinition
        {
            Endpoint = "api/v1/authenticate/refresh",
            Pattern = "api/v1/authenticate/refresh",
            MapTo = "api/v1/authenticate/refresh",
            Method = HttpMethod.Post,
            Meta = new Dictionary<string, string>
            {
                { "allow_anonymous", "true" }
            }
        });

        var archServiceConfig = serviceConfigRepository.FindByNameAsync("arch").Result;
        if (archServiceConfig is null) throw new ServiceConfigNotFoundException();
        archServiceConfig.AddMeta("service_secret", configuration.GetSection("Kundera:Arch_Service_Secret").Value ??
                                                    throw new Exception("Enter Kundera:Arch_Service_Secret in appsettings.json"));

        if (archServiceConfig.EndpointDefinitions.All(definition => definition.Pattern != "api/v1/endpoint-definitions/##/security/permissions"))
        {
            archServiceConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "api/v1/endpoint-definitions/{id}/security/permissions",
                Pattern = "api/v1/endpoint-definitions/##/security/permissions",
                MapTo = "api/v1/endpoint-definitions/{0}/security/permissions",
                Method = HttpMethod.Post,
                Meta = new Dictionary<string, string>
                {
                    { "permissions", "endpoint_definition_add_permission" }
                }
            });
        }

        if (archServiceConfig.EndpointDefinitions.All(definition => definition.Pattern != "api/v1/endpoint-definitions/##/security/allow-anonymous"))
        {
            archServiceConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "api/v1/endpoint-definitions/{id}/security/allow-anonymous",
                Pattern = "api/v1/endpoint-definitions/##/security/allow-anonymous",
                MapTo = "api/v1/endpoint-definitions/{0}/security/allow-anonymous",
                Method = HttpMethod.Post,
                Meta = new Dictionary<string, string>
                {
                    { "permissions", "endpoint_definition_allow_anonymous" }
                }
            });
        }

        serviceConfigRepository.AddAsync(kunderaServiceConfig).Wait();
        serviceConfigRepository.UpdateAsync(archServiceConfig).Wait();
    }
}