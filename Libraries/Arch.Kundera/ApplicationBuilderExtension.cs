﻿using Core.EndpointDefinitions;
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

        if (dbContext.ServiceConfigs.Any(config => config.Name == "kundera")) return;

        var kunderaServiceConfig = new ServiceConfig
        {
            Name = "kundera",
            Primary = true,
            BaseUrl = "http://localhost:5179"
        };

        var kunderaServiceSecret = configuration.GetSection("Kundera:Kundera_Service_Secret").Value ??
                                   throw new Exception("Enter Kundera:Kundera_Service_Secret in appsettings.json");
        kunderaServiceConfig.Meta.Add(new()
        {
            Key = "service_secret",
            Value = kunderaServiceSecret
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

        var archServiceConfig = dbContext.ServiceConfigs
            .Include(config => config.EndpointDefinitions)
            .First(config => config.Name == "arch");
        var archServiceSecret = configuration.GetSection("Kundera:Arch_Service_Secret").Value ??
                                throw new Exception("Enter Kundera:Arch_Service_Secret in appsettings.json");
        archServiceConfig.Meta.Add(new()
        {
            Key = "service_secret",
            Value = archServiceSecret
        });
        archServiceConfig.EndpointDefinitions.Add(new EndpointDefinition
        {
            Endpoint = "endpoint-definitions/{id}/security/permissions",
            Pattern = "endpoint-definitions/##/security/permissions",
            Method = "patch",
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
            Method = "patch",
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