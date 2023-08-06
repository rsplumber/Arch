using Core;
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition is { Pattern: "gateway/api/v1/endpoint-definitions/##", Method: HttpRequestMethods.Get }))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/endpoint-definitions/{id}",
                    Pattern = "gateway/api/v1/endpoint-definitions/##",
                    MapTo = "gateway/api/v1/endpoint-definitions/{0}",
                    Method = HttpRequestMethods.Get,
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition is { Pattern: "gateway/api/v1/endpoint-definitions/##/enable", Method: HttpRequestMethods.Post }))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/endpoint-definitions/{id}/enable",
                    Pattern = "gateway/api/v1/endpoint-definitions/##/enable",
                    MapTo = "gateway/api/v1/endpoint-definitions/{0}/enable",
                    Method = HttpRequestMethods.Post,
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition is { Pattern: "gateway/api/v1/endpoint-definitions/##/disable", Method: HttpRequestMethods.Post }))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/endpoint-definitions/{id}/disable",
                    Pattern = "gateway/api/v1/endpoint-definitions/##/disable",
                    MapTo = "gateway/api/v1/endpoint-definitions/{0}/disable",
                    Method = HttpRequestMethods.Post,
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition is { Pattern: "gateway/api/v1/endpoint-definitions/##", Method: HttpRequestMethods.Delete }))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/endpoint-definitions/{id}",
                    Pattern = "gateway/api/v1/endpoint-definitions/##",
                    MapTo = "gateway/api/v1/endpoint-definitions/{0}",
                    Method = HttpRequestMethods.Delete,
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition is { Pattern: "gateway/api/v1/endpoint-definitions/##", Method: HttpRequestMethods.Put }))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/endpoint-definitions/{id}",
                    Pattern = "gateway/api/v1/endpoint-definitions/##",
                    MapTo = "gateway/api/v1/endpoint-definitions/{0}",
                    Method = HttpRequestMethods.Put,
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition is { Pattern: "gateway/api/v1/endpoint-definitions/required-meta", Method: HttpRequestMethods.Get }))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/endpoint-definitions/required-meta",
                    Pattern = "gateway/api/v1/endpoint-definitions/required-meta",
                    MapTo = "gateway/api/v1/endpoint-definitions/required-meta",
                    Method = HttpRequestMethods.Get,
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition is { Pattern: "gateway/api/v1/service-configs", Method: HttpRequestMethods.Post }))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/service-configs",
                    Pattern = "gateway/api/v1/service-configs",
                    MapTo = "gateway/service-configs",
                    Method = HttpRequestMethods.Post,
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition is { Pattern: "gateway/api/v1/service-configs/required-meta", Method: HttpRequestMethods.Get }))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/service-configs/required-meta",
                    Pattern = "gateway/api/v1/service-configs/required-meta",
                    MapTo = "gateway/service-configs/required-meta",
                    Method = HttpRequestMethods.Get,
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition is { Pattern: "gateway/api/v1/service-configs", Method: HttpRequestMethods.Get }))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/service-configs",
                    Pattern = "gateway/api/v1/service-configs",
                    MapTo = "gateway/service-configs",
                    Method = HttpRequestMethods.Get,
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition is { Pattern: "gateway/api/v1/service-configs/##", Method: HttpRequestMethods.Delete }))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/service-configs/{id}",
                    Pattern = "gateway/api/v1/service-configs/##",
                    MapTo = "gateway/service-configs/{0}",
                    Method = HttpRequestMethods.Delete,
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition is { Pattern: "gateway/api/v1/service-configs/##", Method: HttpRequestMethods.Get }))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/service-configs/{id}",
                    Pattern = "gateway/api/v1/service-configs/##",
                    MapTo = "gateway/service-configs/{0}",
                    Method = HttpRequestMethods.Get,
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition is { Pattern: "gateway/api/v1/service-configs/##", Method: HttpRequestMethods.Put }))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/service-configs/{id}",
                    Pattern = "gateway/api/v1/service-configs/##",
                    MapTo = "gateway/service-configs/{0}",
                    Method = HttpRequestMethods.Put,
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition is { Pattern: "gateway/api/v1/service-configs/##/endpoint-definitions", Method: HttpRequestMethods.Post }))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/service-configs/{id}/endpoint-definitions",
                    Pattern = "gateway/api/v1/service-configs/##/endpoint-definitions",
                    MapTo = "gateway/service-configs/{0}/endpoint-definitions",
                    Method = HttpRequestMethods.Post,
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

            if (!createdConfig.EndpointDefinitions.Any(definition => definition is { Pattern: "gateway/api/v1/service-configs/##/endpoint-definitions", Method: HttpRequestMethods.Get }))
            {
                createdConfig.EndpointDefinitions.Add(new EndpointDefinition
                {
                    Endpoint = "gateway/api/v1/service-configs/{id}/endpoint-definitions",
                    Pattern = "gateway/api/v1/service-configs/##/endpoint-definitions",
                    MapTo = "gateway/service-configs/{0}/endpoint-definitions",
                    Method = HttpRequestMethods.Get,
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