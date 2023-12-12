using Arch.Core.ServiceConfigs;
using Arch.Core.ServiceConfigs.EndpointDefinitions;
using Arch.Data.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Data.EF;

public static class DataExecutionOptionsExtension
{
    public static void UseEntityFramework(this DataExecutionOptions dataExecutionOptions)
    {
        using var serviceScope = dataExecutionOptions.ServiceProvider.GetRequiredService<IServiceScopeFactory>()?.CreateScope();
        var dbContext = serviceScope!.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
        SeedData();
        return;

        void SeedData()
        {
            var archServiceConfig = dbContext.ServiceConfigs
                .Include(serviceConfig => serviceConfig.EndpointDefinitions)
                .FirstOrDefault(config => config.Name == "arch");
            //Seed internal APIs 
            if (archServiceConfig is null)
            {
                archServiceConfig = ServiceConfig.CreatePrimary("arch", "http://localhost:5228");
                archServiceConfig.SetIgnoreDispatch();
                dbContext.ServiceConfigs.Add(archServiceConfig);
                dbContext.SaveChanges();
            }

            if (!archServiceConfig.HasEndpoint("api/v1/endpoint-definitions/##", HttpMethod.Get))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "api/v1/endpoint-definitions/{id}",
                    Pattern = "api/v1/endpoint-definitions/##",
                    MapTo = "api/v1/endpoint-definitions/{0}",
                    Method = HttpMethod.Get,
                    Meta = new Dictionary<string, string>
                    {
                        { "permissions", "details_endpoint_definition" }
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("api/v1/endpoint-definitions/##/enable", HttpMethod.Post))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "api/v1/endpoint-definitions/{id}/enable",
                    Pattern = "api/v1/endpoint-definitions/##/enable",
                    MapTo = "api/v1/endpoint-definitions/{0}/enable",
                    Method = HttpMethod.Post,
                    Meta = new Dictionary<string, string>
                    {
                        { "permissions", "enable_endpoint_definition" }
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("api/v1/endpoint-definitions/##/disable", HttpMethod.Post))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "api/v1/endpoint-definitions/{id}/disable",
                    Pattern = "api/v1/endpoint-definitions/##/disable",
                    MapTo = "api/v1/endpoint-definitions/{0}/disable",
                    Method = HttpMethod.Post,
                    Meta = new Dictionary<string, string>
                    {
                        { "permissions", "disable_endpoint_definition" }
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("api/v1/endpoint-definitions/##", HttpMethod.Delete))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "api/v1/endpoint-definitions/{id}",
                    Pattern = "api/v1/endpoint-definitions/##",
                    MapTo = "api/v1/endpoint-definitions/{0}",
                    Method = HttpMethod.Delete,
                    Meta = new Dictionary<string, string>
                    {
                        { "permissions", "delete_endpoint_definition" }
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("api/v1/endpoint-definitions/##", HttpMethod.Put))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "api/v1/endpoint-definitions/{id}",
                    Pattern = "api/v1/endpoint-definitions/##",
                    MapTo = "api/v1/endpoint-definitions/{0}",
                    Method = HttpMethod.Put,
                    Meta = new Dictionary<string, string>
                    {
                        { "permissions", "update_endpoint_definition" }
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("api/v1/endpoint-definitions/required-meta", HttpMethod.Get))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "api/v1/endpoint-definitions/required-meta",
                    Pattern = "api/v1/endpoint-definitions/required-meta",
                    MapTo = "api/v1/endpoint-definitions/required-meta",
                    Method = HttpMethod.Get,
                    Meta = new Dictionary<string, string>
                    {
                        { "permissions", "required-meta_endpoint_definition" }
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("api/v1/service-configs", HttpMethod.Post))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "api/v1/service-configs",
                    Pattern = "api/v1/service-configs",
                    MapTo = "service-configs",
                    Method = HttpMethod.Post,
                    Meta = new Dictionary<string, string>
                    {
                        { "permissions", "create_service_config" }
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("api/v1/service-configs/required-meta", HttpMethod.Get))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "api/v1/service-configs/required-meta",
                    Pattern = "api/v1/service-configs/required-meta",
                    MapTo = "service-configs/required-meta",
                    Method = HttpMethod.Get,
                    Meta = new Dictionary<string, string>
                    {
                        { "permissions", "required-meta_service_config" }
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("api/v1/service-configs", HttpMethod.Get))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "api/v1/service-configs",
                    Pattern = "api/v1/service-configs",
                    MapTo = "service-configs",
                    Method = HttpMethod.Get,
                    Meta = new Dictionary<string, string>
                    {
                        { "permissions", "list_service_config" }
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("api/v1/service-configs/##", HttpMethod.Delete))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "api/v1/service-configs/{id}",
                    Pattern = "api/v1/service-configs/##",
                    MapTo = "service-configs/{0}",
                    Method = HttpMethod.Delete,
                    Meta = new Dictionary<string, string>
                    {
                        { "permissions", "delete_service_config" }
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("api/v1/service-configs/##", HttpMethod.Get))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "api/v1/service-configs/{id}",
                    Pattern = "api/v1/service-configs/##",
                    MapTo = "service-configs/{0}",
                    Method = HttpMethod.Get,
                    Meta = new Dictionary<string, string>
                    {
                        { "permissions", "details_service_config" }
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("api/v1/service-configs/##", HttpMethod.Put))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "api/v1/service-configs/{id}",
                    Pattern = "api/v1/service-configs/##",
                    MapTo = "service-configs/{0}",
                    Method = HttpMethod.Put,
                    Meta = new Dictionary<string, string>
                    {
                        { "permissions", "update_service_config" }
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("api/v1/service-configs/##/endpoint-definitions", HttpMethod.Post))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "api/v1/service-configs/{id}/endpoint-definitions",
                    Pattern = "api/v1/service-configs/##/endpoint-definitions",
                    MapTo = "service-configs/{0}/endpoint-definitions",
                    Method = HttpMethod.Post,
                    Meta = new Dictionary<string, string>
                    {
                        { "permissions", "create_endpoint_definition" }
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("api/v1/service-configs/##/endpoint-definitions", HttpMethod.Get))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "api/v1/service-configs/{id}/endpoint-definitions",
                    Pattern = "api/v1/service-configs/##/endpoint-definitions",
                    MapTo = "service-configs/{0}/endpoint-definitions",
                    Method = HttpMethod.Get,
                    Meta = new Dictionary<string, string>
                    {
                        { "permissions", "list_endpoint_definition" }
                    }
                });
            }

            dbContext.ServiceConfigs.Update(archServiceConfig);
            dbContext.SaveChanges();
        }
    }
}