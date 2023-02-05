using Application.Middlewares;
using Arch.Clerk;
using Arch.Kundera;
using Core.EndpointDefinitions.Containers;
using Core.EndpointDefinitions.Resolvers;
using Core.EndpointDefinitions.Services;
using Core.Metas;
using Core.ServiceConfigs;
using Core.ServiceConfigs.Services;
using Data.Sql;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using EndpointDefinition = Core.EndpointDefinitions.EndpointDefinition;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel();
builder.WebHost.UseUrls("http://+:5220");

builder.Services.AddHttpClient("default", _ => { });

builder.Services.AddSingleton<ExceptionHandlerMiddleware>();
builder.Services.AddSingleton<RequestExtractorMiddleware>();
builder.Services.AddKundera(builder.Configuration);
builder.Services.AddClerkAccounting(builder.Configuration);
builder.Services.AddSingleton<RequestDispatcherMiddleware>();

builder.Services.AddSingleton<IEndpointDefinitionResolver, EndpointDefinitionResolver>();
builder.Services.AddSingleton<IEndpointPatternTree, InMemoryEndpointPatternTree>();
builder.Services.AddSingleton<IEndpointDefinitionContainer, InMemoryEndpointDefinitionContainer>();

builder.Services.AddSingleton<IEndpointPatternTree, InMemoryEndpointPatternTree>();

builder.Services.AddScoped<IEndpointDefinitionService, EndpointDefinitionService>();
builder.Services.AddScoped<IServiceConfigService, ServiceConfigService>();
builder.Services.AddScoped<IContainerInitializer, InMemoryContainerInitializer>();
builder.Services.AddData(builder.Configuration);
builder.Services.AddCors();
builder.Services.AddFastEndpoints();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<RequestExtractorMiddleware>();
app.UseKundera();
app.UseClerkAccounting();
app.UseMiddleware<RequestDispatcherMiddleware>();

using (var serviceScope = app.Services.GetService<IServiceScopeFactory>()?.CreateScope())
{
    if (serviceScope == null) return;
    try
    {
        var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.Migrate();

        var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (!dbContext.ServiceConfigs.Any(config => config.Name == "arch"))
        {
            var serviceConfig = new ServiceConfig
            {
                Name = "arch"
            };
            serviceConfig.Meta.Add(new Meta
            {
                Key = "base_url",
                Value = "http://localhost:5228"
            });
            serviceConfig.Meta.Add(new Meta
            {
                Key = "service_secret",
                Value = "9C1BE6A9A93D6498B10506743BD6A30DCAEB6F625F9E92C7400BC6DE56B625E09A34A6CF8C29AD0EC335BC2AF5D63E8C"
            });
            dbContext.ServiceConfigs.Add(serviceConfig);
            dbContext.SaveChanges();
            var createdConfig = await dbContext.ServiceConfigs
                .Include(config => config.EndpointDefinitions)
                .ThenInclude(definition => definition.Meta)
                .FirstAsync(config => config.Id == serviceConfig.Id);
            createdConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "endpoint-definitions/{id}",
                Pattern = "endpoint-definitions/##",
                Method = "get",
                Meta = new List<Meta>
                {
                    new()
                    {
                        Key = "permissions",
                        Value = "arch_details_endpoint_definition"
                    },
                    new()
                    {
                        Key = "ignore_dispatch",
                        Value = "true"
                    }
                }
            });
            createdConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "endpoint-definitions/{id}",
                Pattern = "endpoint-definitions/##",
                Method = "delete",
                Meta = new List<Meta>
                {
                    new()
                    {
                        Key = "permissions",
                        Value = "arch_delete_endpoint_definition"
                    },
                    new()
                    {
                        Key = "ignore_dispatch",
                        Value = "true"
                    }
                }
            });
            createdConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "endpoint-definitions/{id}",
                Pattern = "endpoint-definitions/##",
                Method = "put",
                Meta = new List<Meta>
                {
                    new()
                    {
                        Key = "permissions",
                        Value = "arch_update_endpoint_definition"
                    },
                    new()
                    {
                        Key = "ignore_dispatch",
                        Value = "true"
                    }
                }
            });
            createdConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "service-configs",
                Pattern = "service-configs",
                Method = "post",
                Meta = new List<Meta>
                {
                    new()
                    {
                        Key = "permissions",
                        Value = "arch_create_service_config"
                    },
                    new()
                    {
                        Key = "ignore_dispatch",
                        Value = "true"
                    }
                }
            });
            createdConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "service-configs",
                Pattern = "service-configs",
                Method = "get",
                Meta = new List<Meta>
                {
                    new()
                    {
                        Key = "permissions",
                        Value = "arch_list_service_config"
                    },
                    new()
                    {
                        Key = "ignore_dispatch",
                        Value = "true"
                    }
                },
            });
            createdConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "service-configs/{id}",
                Pattern = "service-configs/##",
                Method = "delete",
                Meta = new List<Meta>
                {
                    new()
                    {
                        Key = "permissions",
                        Value = "arch_delete_service_config"
                    },
                    new()
                    {
                        Key = "ignore_dispatch",
                        Value = "true"
                    }
                }
            });
            createdConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "service-configs/{id}",
                Pattern = "service-configs/##",
                Method = "get",
                Meta = new List<Meta>
                {
                    new()
                    {
                        Key = "permissions",
                        Value = "arch_details_service_config"
                    },
                    new()
                    {
                        Key = "ignore_dispatch",
                        Value = "true"
                    }
                }
            });
            createdConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "service-configs/{id}",
                Pattern = "service-configs/##",
                Method = "put",
                Meta = new List<Meta>
                {
                    new()
                    {
                        Key = "permissions",
                        Value = "arch_update_service_config"
                    },
                    new()
                    {
                        Key = "ignore_dispatch",
                        Value = "true"
                    }
                }
            });
            createdConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "service-configs/{id}/endpoint-definitions",
                Pattern = "service-configs/##/endpoint-definitions",
                Method = "post",
                Meta = new List<Meta>
                {
                    new()
                    {
                        Key = "permissions",
                        Value = "arch_create_endpoint_definition"
                    },
                    new()
                    {
                        Key = "ignore_dispatch",
                        Value = "true"
                    }
                }
            });
            createdConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "service-configs/{id}/endpoint-definitions",
                Pattern = "service-configs/##/endpoint-definitions",
                Method = "get",
                Meta = new List<Meta>
                {
                    new()
                    {
                        Key = "permissions",
                        Value = "arch_list_endpoint_definition"
                    },
                    new()
                    {
                        Key = "ignore_dispatch",
                        Value = "true"
                    }
                }
            });
            dbContext.ServiceConfigs.Update(createdConfig);
            dbContext.SaveChanges();
        }
    }
    catch (Exception)
    {
        // ignored
    }
}

await InitializeInMemoryContainers();

app.UseCors(b => b.AllowAnyHeader()
    .AllowAnyMethod()
    .SetIsOriginAllowed(_ => true)
    .AllowCredentials());

app.UseFastEndpoints();

await app.RunAsync();

async Task InitializeInMemoryContainers()
{
    using var serviceScope = app.Services.GetService<IServiceScopeFactory>()?.CreateScope();
    var dbContext = serviceScope!.ServiceProvider.GetRequiredService<AppDbContext>();
    var configs = await dbContext.ServiceConfigs
        .Include(config => config.Meta)
        .Include(config => config.EndpointDefinitions)
        .ThenInclude(definition => definition.Meta)
        .ToListAsync();

    var containerInitializer = serviceScope.ServiceProvider.GetRequiredService<IContainerInitializer>();
    await containerInitializer.InitializeAsync(configs);
}