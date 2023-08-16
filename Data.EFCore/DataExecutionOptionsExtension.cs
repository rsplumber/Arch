using Core.EndpointDefinitions;
using Core.Metas;
using Core.ServiceConfigs;
using Data.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Data.EFCore;

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
                .Include(serviceConfig => serviceConfig.Meta)
                .Include(serviceConfig => serviceConfig.EndpointDefinitions)
                .ThenInclude(endpoint => endpoint.Meta)
                .FirstOrDefault(config => config.Name == "arch");
            //Seed internal APIs 
            if (archServiceConfig is null)
            {
                archServiceConfig = ServiceConfig.CreatePrimary("arch", "http://localhost:5228");
                archServiceConfig.SetIgnoreDispatch();
                dbContext.ServiceConfigs.Add(archServiceConfig);
                dbContext.SaveChanges();
            }

            if (!archServiceConfig.HasEndpoint("gateway/api/v1/endpoint-definitions/##", HttpMethod.Get))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/endpoint-definitions/{id}",
                    Pattern = "gateway/api/v1/endpoint-definitions/##",
                    MapTo = "gateway/api/v1/endpoint-definitions/{0}",
                    Method = HttpMethod.Get,
                    Meta = new List<Meta>
                    {
                        new()
                        {
                            Key = "permissions",
                            Value = "details_endpoint_definition"
                        },
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("gateway/api/v1/endpoint-definitions/##/enable", HttpMethod.Post))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/endpoint-definitions/{id}/enable",
                    Pattern = "gateway/api/v1/endpoint-definitions/##/enable",
                    MapTo = "gateway/api/v1/endpoint-definitions/{0}/enable",
                    Method = HttpMethod.Post,
                    Meta = new List<Meta>
                    {
                        new()
                        {
                            Key = "permissions",
                            Value = "enable_endpoint_definition"
                        },
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("gateway/api/v1/endpoint-definitions/##/disable", HttpMethod.Post))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/endpoint-definitions/{id}/disable",
                    Pattern = "gateway/api/v1/endpoint-definitions/##/disable",
                    MapTo = "gateway/api/v1/endpoint-definitions/{0}/disable",
                    Method = HttpMethod.Post,
                    Meta = new List<Meta>
                    {
                        new()
                        {
                            Key = "permissions",
                            Value = "disable_endpoint_definition"
                        },
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("gateway/api/v1/endpoint-definitions/##", HttpMethod.Delete))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/endpoint-definitions/{id}",
                    Pattern = "gateway/api/v1/endpoint-definitions/##",
                    MapTo = "gateway/api/v1/endpoint-definitions/{0}",
                    Method = HttpMethod.Delete,
                    Meta = new List<Meta>
                    {
                        new()
                        {
                            Key = "permissions",
                            Value = "delete_endpoint_definition"
                        },
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("gateway/api/v1/endpoint-definitions/##", HttpMethod.Put))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/endpoint-definitions/{id}",
                    Pattern = "gateway/api/v1/endpoint-definitions/##",
                    MapTo = "gateway/api/v1/endpoint-definitions/{0}",
                    Method = HttpMethod.Put,
                    Meta = new List<Meta>
                    {
                        new()
                        {
                            Key = "permissions",
                            Value = "update_endpoint_definition"
                        },
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("gateway/api/v1/endpoint-definitions/required-meta", HttpMethod.Get))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/endpoint-definitions/required-meta",
                    Pattern = "gateway/api/v1/endpoint-definitions/required-meta",
                    MapTo = "gateway/api/v1/endpoint-definitions/required-meta",
                    Method = HttpMethod.Get,
                    Meta = new List<Meta>
                    {
                        new()
                        {
                            Key = "permissions",
                            Value = "required-meta_endpoint_definition"
                        },
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("gateway/api/v1/service-configs", HttpMethod.Post))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/service-configs",
                    Pattern = "gateway/api/v1/service-configs",
                    MapTo = "gateway/service-configs",
                    Method = HttpMethod.Post,
                    Meta = new List<Meta>
                    {
                        new()
                        {
                            Key = "permissions",
                            Value = "create_service_config"
                        }
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("gateway/api/v1/service-configs/required-meta", HttpMethod.Get))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/service-configs/required-meta",
                    Pattern = "gateway/api/v1/service-configs/required-meta",
                    MapTo = "gateway/service-configs/required-meta",
                    Method = HttpMethod.Get,
                    Meta = new List<Meta>
                    {
                        new()
                        {
                            Key = "permissions",
                            Value = "required-meta_service_config"
                        },
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("gateway/api/v1/service-configs", HttpMethod.Get))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/service-configs",
                    Pattern = "gateway/api/v1/service-configs",
                    MapTo = "gateway/service-configs",
                    Method = HttpMethod.Get,
                    Meta = new List<Meta>
                    {
                        new()
                        {
                            Key = "permissions",
                            Value = "list_service_config"
                        },
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("gateway/api/v1/service-configs/##", HttpMethod.Delete))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/service-configs/{id}",
                    Pattern = "gateway/api/v1/service-configs/##",
                    MapTo = "gateway/service-configs/{0}",
                    Method = HttpMethod.Delete,
                    Meta = new List<Meta>
                    {
                        new()
                        {
                            Key = "permissions",
                            Value = "delete_service_config"
                        },
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("gateway/api/v1/service-configs/##", HttpMethod.Get))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/service-configs/{id}",
                    Pattern = "gateway/api/v1/service-configs/##",
                    MapTo = "gateway/service-configs/{0}",
                    Method = HttpMethod.Get,
                    Meta = new List<Meta>
                    {
                        new()
                        {
                            Key = "permissions",
                            Value = "details_service_config"
                        },
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("gateway/api/v1/service-configs/##", HttpMethod.Put))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/service-configs/{id}",
                    Pattern = "gateway/api/v1/service-configs/##",
                    MapTo = "gateway/service-configs/{0}",
                    Method = HttpMethod.Put,
                    Meta = new List<Meta>
                    {
                        new()
                        {
                            Key = "permissions",
                            Value = "update_service_config"
                        },
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("gateway/api/v1/service-configs/##/endpoint-definitions", HttpMethod.Post))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/service-configs/{id}/endpoint-definitions",
                    Pattern = "gateway/api/v1/service-configs/##/endpoint-definitions",
                    MapTo = "gateway/service-configs/{0}/endpoint-definitions",
                    Method = HttpMethod.Post,
                    Meta = new List<Meta>
                    {
                        new()
                        {
                            Key = "permissions",
                            Value = "create_endpoint_definition"
                        },
                    }
                });
            }

            if (!archServiceConfig.HasEndpoint("gateway/api/v1/service-configs/##/endpoint-definitions", HttpMethod.Get))
            {
                archServiceConfig.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/service-configs/{id}/endpoint-definitions",
                    Pattern = "gateway/api/v1/service-configs/##/endpoint-definitions",
                    MapTo = "gateway/service-configs/{0}/endpoint-definitions",
                    Method = HttpMethod.Get,
                    Meta = new List<Meta>
                    {
                        new()
                        {
                            Key = "permissions",
                            Value = "list_endpoint_definition"
                        },
                    }
                });
            }

            dbContext.ServiceConfigs.Update(archServiceConfig);
            dbContext.SaveChanges();
        }
    }
}