using Application.Middlewares;
using Arch.Clerk;
using Arch.Kundera;
using Core.EndpointDefinitions.Containers;
using Core.EndpointDefinitions.Containers.Resolvers;
using Core.EndpointDefinitions.Services;
using Core.Library;
using Core.Metas;
using Core.ServiceConfigs;
using Core.ServiceConfigs.Services;
using Data.Sql;
using Elastic.Apm.AspNetCore;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using EndpointDefinition = Core.EndpointDefinitions.EndpointDefinition;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel();
builder.WebHost.UseUrls("http://+:5228");
builder.Services.AddCors();

builder.Services.AddHttpClient("arch", _ => { });

builder.Services.AddArchMiddleware<ExceptionHandlerMiddleware>();
builder.Services.AddArchMiddleware<RequestExtractorMiddleware>();
builder.Services.AddKundera(builder.Configuration);
builder.Services.AddClerkAccounting(builder.Configuration);
builder.Services.AddArchMiddleware<RequestDispatcherMiddleware>();
builder.Services.AddArchMiddleware<ResponseHandlerMiddleware>();

builder.Services.AddSingleton<IEndpointDefinitionResolver, EndpointDefinitionResolver>();
builder.Services.AddSingleton<IEndpointPatternTree, InMemoryEndpointPatternTree>();
builder.Services.AddSingleton<IEndpointDefinitionContainer, InMemoryEndpointDefinitionContainer>();
builder.Services.AddSingleton<IEndpointPatternTree, InMemoryEndpointPatternTree>();
builder.Services.AddScoped<IEndpointDefinitionService, EndpointDefinitionService>();
builder.Services.AddScoped<IServiceConfigService, ServiceConfigService>();
builder.Services.AddScoped<IContainerInitializer, InMemoryContainerInitializer>();

builder.Services.AddData(builder.Configuration);

builder.Services.AddFastEndpoints();

var app = builder.Build();

app.UseCors(b => b.AllowAnyHeader()
    .AllowAnyMethod()
    .SetIsOriginAllowed(_ => true)
    .AllowCredentials());

await SeedDataAsync();
await InitializeInMemoryContainers();

app.UseArchMiddleware<ExceptionHandlerMiddleware>();
app.UseArchMiddleware<RequestExtractorMiddleware>();
app.UseKundera(builder.Configuration);
app.UseClerkAccounting(builder.Configuration);
app.UseArchMiddleware<RequestDispatcherMiddleware>();
app.UseElasticApm(builder.Configuration);
app.UseArchMiddleware<ResponseHandlerMiddleware>();

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

async Task SeedDataAsync()
{
    using var serviceScope = app.Services.GetService<IServiceScopeFactory>()?.CreateScope();
    var context = serviceScope!.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();

    //Seed internal APIs 
    var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
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

    var createdConfig = await dbContext.ServiceConfigs
        .Include(config => config.EndpointDefinitions)
        .ThenInclude(definition => definition.Meta)
        .FirstAsync(config => config.Id == serviceConfig.Id);

    createdConfig.EndpointDefinitions.Add(new EndpointDefinition
    {
        Endpoint = "endpoint-definitions/{id}",
        Pattern = "endpoint-definitions/##",
        Method = HttpRequestMethods.Get,
        Meta = new List<Meta>
        {
            new()
            {
                Key = "permissions",
                Value = "arch_details_endpoint_definition"
            },
        }
    });

    createdConfig.EndpointDefinitions.Add(new EndpointDefinition
    {
        Endpoint = "endpoint-definitions/{id}/enable",
        Pattern = "endpoint-definitions/##/enable",
        Method = HttpRequestMethods.Patch,
        Meta = new List<Meta>
        {
            new()
            {
                Key = "permissions",
                Value = "arch_enable_endpoint_definition"
            },
        }
    });

    createdConfig.EndpointDefinitions.Add(new EndpointDefinition
    {
        Endpoint = "endpoint-definitions/{id}/disable",
        Pattern = "endpoint-definitions/##/disable",
        Method = HttpRequestMethods.Patch,
        Meta = new List<Meta>
        {
            new()
            {
                Key = "permissions",
                Value = "arch_disable_endpoint_definition"
            },
        }
    });


    createdConfig.EndpointDefinitions.Add(new EndpointDefinition
    {
        Endpoint = "endpoint-definitions/{id}",
        Pattern = "endpoint-definitions/##",
        Method = HttpRequestMethods.Delete,
        Meta = new List<Meta>
        {
            new()
            {
                Key = "permissions",
                Value = "arch_delete_endpoint_definition"
            },
        }
    });
    createdConfig.EndpointDefinitions.Add(new EndpointDefinition
    {
        Endpoint = "endpoint-definitions/{id}",
        Pattern = "endpoint-definitions/##",
        Method = HttpRequestMethods.Put,
        Meta = new List<Meta>
        {
            new()
            {
                Key = "permissions",
                Value = "arch_update_endpoint_definition"
            },
        }
    });
    createdConfig.EndpointDefinitions.Add(new EndpointDefinition
    {
        Endpoint = "endpoint-definitions/required-meta",
        Pattern = "endpoint-definitions/required-meta",
        Method = HttpRequestMethods.Get,
        Meta = new List<Meta>
        {
            new()
            {
                Key = "permissions",
                Value = "arch_required-meta_endpoint_definition"
            },
        }
    });
    createdConfig.EndpointDefinitions.Add(new EndpointDefinition
    {
        Endpoint = "service-configs",
        Pattern = "service-configs",
        Method = HttpRequestMethods.Post,
        Meta = new List<Meta>
        {
            new()
            {
                Key = "permissions",
                Value = "arch_create_service_config"
            },
        }
    });
    createdConfig.EndpointDefinitions.Add(new EndpointDefinition
    {
        Endpoint = "service-configs/required-meta",
        Pattern = "service-configs/required-meta",
        Method = HttpRequestMethods.Get,
        Meta = new List<Meta>
        {
            new()
            {
                Key = "permissions",
                Value = "arch_required-meta_service_config"
            },
        }
    });
    createdConfig.EndpointDefinitions.Add(new EndpointDefinition
    {
        Endpoint = "service-configs",
        Pattern = "service-configs",
        Method = HttpRequestMethods.Get,
        Meta = new List<Meta>
        {
            new()
            {
                Key = "permissions",
                Value = "arch_list_service_config"
            },
        }
    });
    createdConfig.EndpointDefinitions.Add(new EndpointDefinition
    {
        Endpoint = "service-configs/{id}",
        Pattern = "service-configs/##",
        Method = HttpRequestMethods.Delete,
        Meta = new List<Meta>
        {
            new()
            {
                Key = "permissions",
                Value = "arch_delete_service_config"
            },
        }
    });
    createdConfig.EndpointDefinitions.Add(new EndpointDefinition
    {
        Endpoint = "service-configs/{id}",
        Pattern = "service-configs/##",
        Method = HttpRequestMethods.Get,
        Meta = new List<Meta>
        {
            new()
            {
                Key = "permissions",
                Value = "arch_details_service_config"
            },
        }
    });
    createdConfig.EndpointDefinitions.Add(new EndpointDefinition
    {
        Endpoint = "service-configs/{id}",
        Pattern = "service-configs/##",
        Method = HttpRequestMethods.Put,
        Meta = new List<Meta>
        {
            new()
            {
                Key = "permissions",
                Value = "arch_update_service_config"
            },
        }
    });
    createdConfig.EndpointDefinitions.Add(new EndpointDefinition
    {
        Endpoint = "service-configs/{id}/endpoint-definitions",
        Pattern = "service-configs/##/endpoint-definitions",
        Method = HttpRequestMethods.Post,
        Meta = new List<Meta>
        {
            new()
            {
                Key = "permissions",
                Value = "arch_create_endpoint_definition"
            },
        }
    });
    createdConfig.EndpointDefinitions.Add(new EndpointDefinition
    {
        Endpoint = "service-configs/{id}/endpoint-definitions",
        Pattern = "service-configs/##/endpoint-definitions",
        Method = HttpRequestMethods.Get,
        Meta = new List<Meta>
        {
            new()
            {
                Key = "permissions",
                Value = "arch_list_endpoint_definition"
            },
        }
    });
    dbContext.ServiceConfigs.Update(createdConfig);
    dbContext.SaveChanges();
}