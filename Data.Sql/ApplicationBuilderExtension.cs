using Core.Entities.EndpointDefinitions;
using Core.Entities.Metas;
using Core.Entities.ServiceConfigs;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Data.Sql;

public static class ApplicationBuilderExtension
{
    public static void UseData(this IApplicationBuilder app, IConfiguration configuration)
    {
        using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
        var dbContext = serviceScope!.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
        SeedData();
        return;

        void SeedData()
        {
            //Seed internal APIs 
            if (dbContext.ServiceConfigs.Any(config => config.Name == "arch")) return;
            var serviceConfig = new ServiceConfig
            {
                Name = "arch",
                Primary = true,
                BaseUrl = "http://localhost:5228"
            };

            //Ignore dispatching for internal APIs to prevent dispatching loop 
            serviceConfig.Meta.Add(new()
            {
                Key = "ignore_dispatch",
                Value = "true"
            });

            dbContext.ServiceConfigs.Add(serviceConfig);
            dbContext.SaveChanges();

            var createdConfig = dbContext.ServiceConfigs
                .Include(config => config.EndpointDefinitions)
                .ThenInclude(definition => definition.Meta)
                .First(config => config.Id == serviceConfig.Id);

            if (!createdConfig.EndpointDefinitions.Any(definition => definition.Pattern == "gateway/api/v1/endpoint-definitions/##" && definition.Method == HttpMethod.Get))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition.Pattern == "gateway/api/v1/endpoint-definitions/##/enable"&& definition.Method == HttpMethod.Post ))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition.Pattern == "gateway/api/v1/endpoint-definitions/##/disable"&& definition.Method == HttpMethod.Post))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition.Pattern == "gateway/api/v1/endpoint-definitions/##"&& definition.Method == HttpMethod.Delete ))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition.Pattern == "gateway/api/v1/endpoint-definitions/##"&& definition.Method == HttpMethod.Put))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition.Pattern == "gateway/api/v1/endpoint-definitions/required-meta"&& definition.Method == HttpMethod.Get))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition.Pattern == "gateway/api/v1/service-configs"&& definition.Method == HttpMethod.Post))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition.Pattern == "gateway/api/v1/service-configs/required-meta"&& definition.Method == HttpMethod.Get))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition.Pattern == "gateway/api/v1/service-configs"&& definition.Method == HttpMethod.Get))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition.Pattern == "gateway/api/v1/service-configs/##"&& definition.Method == HttpMethod.Delete))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition.Pattern == "gateway/api/v1/service-configs/##"&& definition.Method == HttpMethod.Get))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition.Pattern == "gateway/api/v1/service-configs/##"&& definition.Method == HttpMethod.Put))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition.Pattern == "gateway/api/v1/service-configs/##/endpoint-definitions"&& definition.Method == HttpMethod.Post))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition.Pattern == "gateway/api/v1/service-configs/##/endpoint-definitions"&& definition.Method == HttpMethod.Get))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
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

            dbContext.ServiceConfigs.Update(createdConfig);
            dbContext.SaveChanges();
        }
    }
}