using System.Text.Json;
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
using Data.InMemory;
using Data.Sql;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using EndpointDefinition = Core.EndpointDefinitions.EndpointDefinition;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel();
builder.WebHost.ConfigureKestrel((_, options) =>
{
    options.ListenAnyIP(5228, _ => { });
    // options.ListenAnyIP(5228, listenOptions =>
    // {
    //     listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
    //     listenOptions.UseHttps();
    // });
});
builder.Services.AddCors();
builder.Services.AddHttpClient("arch", _ => { });

builder.Services.AddArchMiddleware<ExceptionHandlerMiddleware>();
builder.Services.AddArchMiddleware<RequestExtractorMiddleware>();
builder.Services.AddKundera(builder.Configuration);
builder.Services.AddClerkAccounting(builder.Configuration);
builder.Services.AddArchMiddleware<RequestDispatcherMiddleware>();
builder.Services.AddArchMiddleware<ResponseHandlerMiddleware>();

builder.Services.AddSingleton<IEndpointDefinitionResolver, EndpointDefinitionResolver>();
builder.Services.AddScoped<IEndpointDefinitionService, EndpointDefinitionService>();
builder.Services.AddScoped<IServiceConfigService, ServiceConfigService>();

builder.Services.AddCap(options =>
{
    options.UseRabbitMQ(op =>
    {
        op.HostName = builder.Configuration.GetValue<string>("RabbitMQ:HostName") ?? throw new ArgumentNullException("RabbitMQ:HostName", "Enter RabbitMQ:HostName in app settings");
        op.UserName = builder.Configuration.GetValue<string>("RabbitMQ:UserName") ?? throw new ArgumentNullException("RabbitMQ:UserName", "Enter RabbitMQ:UserName in app settings");
        op.Password = builder.Configuration.GetValue<string>("RabbitMQ:Password") ?? throw new ArgumentNullException("RabbitMQ:Password", "Enter RabbitMQ:UserName in app settings");
        op.ExchangeName = builder.Configuration.GetValue<string>("RabbitMQ:ExchangeName") ?? throw new ArgumentNullException("RabbitMQ:ExchangeName", "Enter RabbitMQ:ExchangeName in app settings");
    });
    options.UsePostgreSql(sqlOptions =>
    {
        sqlOptions.ConnectionString = builder.Configuration.GetConnectionString("default") ?? throw new ArgumentNullException("connectionString", "Enter connection string in app settings");
        sqlOptions.Schema = "events";
    });
});

builder.Services.AddData(builder.Configuration);
builder.Services.AddInMemoryDataContainers();

builder.Services.AddFastEndpoints();
// builder.Services.AddAuthentication(KunderaDefaults.Scheme)
//     .AddKundera(builder.Configuration);
// builder.Services.AddAuthorization();
var app = builder.Build();

app.UseCors(b => b.AllowAnyHeader()
    .AllowAnyMethod()
    .SetIsOriginAllowed(_ => true)
    .AllowCredentials());

await SeedDataAsync();

app.UseArchMiddleware<ExceptionHandlerMiddleware>();
app.UseArchMiddleware<RequestExtractorMiddleware>();
app.UseKundera(builder.Configuration);
app.UseClerkAccounting(builder.Configuration);
app.UseArchMiddleware<RequestDispatcherMiddleware>();
// app.UseAllElasticApm(builder.Configuration);
app.UseArchMiddleware<ResponseHandlerMiddleware>();

await InitializeInMemoryContainers();

app.UseFastEndpoints(config =>
{
    config.Serializer.Options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    config.Endpoints.RoutePrefix = "gateway/api";
    config.Versioning.Prefix = "v";
    config.Versioning.PrependToRoute = true;
});

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
                    Value = "arch_details_endpoint_definition"
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
                    Value = "arch_enable_endpoint_definition"
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
                    Value = "arch_disable_endpoint_definition"
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
                    Value = "arch_delete_endpoint_definition"
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
                    Value = "arch_update_endpoint_definition"
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
                    Value = "arch_required-meta_endpoint_definition"
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
                    Value = "arch_create_service_config"
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
                    Value = "arch_required-meta_service_config"
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
                    Value = "arch_list_service_config"
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
                    Value = "arch_delete_service_config"
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
                    Value = "arch_details_service_config"
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
                    Value = "arch_update_service_config"
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
                    Value = "arch_create_endpoint_definition"
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
                    Value = "arch_list_endpoint_definition"
                },
            }
        });
    }

    dbContext.ServiceConfigs.Update(createdConfig);
    dbContext.SaveChanges();
}