using Core.EndpointDefinitions.Containers;
using Core.EndpointDefinitions.Resolvers;
using Core.EndpointDefinitions.Services;
using Core.Metas;
using Core.ServiceConfigs;
using Core.ServiceConfigs.Services;
using Data.Sql;
using FastEndpoints;
using Management;
using Microsoft.EntityFrameworkCore;
using EndpointDefinition = Core.EndpointDefinitions.EndpointDefinition;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel();
builder.WebHost.UseUrls("http://+:5229");

builder.Services.AddSingleton<IEndpointDefinitionResolver, EndpointDefinitionResolver>();
builder.Services.AddSingleton<IEndpointPatternTree, InMemoryEndpointPatternTree>();
builder.Services.AddSingleton<IEndpointDefinitionContainer, InMemoryEndpointDefinitionContainer>();

builder.Services.AddSingleton<IEndpointPatternTree, InMemoryEndpointPatternTree>();

builder.Services.AddScoped<IEndpointDefinitionService, EndpointDefinitionService>();
builder.Services.AddScoped<IServiceConfigService, ServiceConfigService>();

builder.Services.AddData(builder.Configuration);
builder.Services.AddDbContext<ManagementDbContext>(
    b => b.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddCors();
builder.Services.AddFastEndpoints();

var app = builder.Build();

using (var serviceScope = app.Services.GetService<IServiceScopeFactory>()?.CreateScope())
{
    if (serviceScope == null) return;
    try
    {
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<ManagementDbContext>();
        if (!dbContext.ServiceConfigs.Any(config => config.Name == "arch"))
        {
            var serviceConfig = new ServiceConfig
            {
                Name = "arch"
            };
            serviceConfig.Meta.Add(new Meta
            {
                Id = "base_url",
                Value = "http://localhost:5229"
            });
            dbContext.ServiceConfigs.Add(serviceConfig);
            dbContext.SaveChanges();
            serviceConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "api/endpoint-definitions/{id}",
                Pattern = "api/endpoint-definitions/##",
                Method = "get"
            });
            serviceConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "api/endpoint-definitions/{id}",
                Pattern = "api/endpoint-definitions/##",
                Method = "delete"
            });
            serviceConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "api/endpoint-definitions/{id}",
                Pattern = "api/endpoint-definitions/##",
                Method = "put"
            });
            serviceConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "api/service-configs",
                Pattern = "api/service-configs",
                Method = "post"
            });
            serviceConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "api/service-configs",
                Pattern = "api/service-configs",
                Method = "get"
            });
            serviceConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "api/service-configs/{id}",
                Pattern = "api/service-configs/##",
                Method = "delete"
            });
            serviceConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "api/service-configs/{id}",
                Pattern = "api/service-configs/##",
                Method = "get"
            });
            serviceConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "api/service-configs/{id}",
                Pattern = "api/service-configs/##",
                Method = "put"
            });
            serviceConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "api/service-configs/{id}/endpoint-definitions",
                Pattern = "api/service-configs/##/endpoint-definitions",
                Method = "post"
            });
            serviceConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "api/service-configs/{id}/endpoint-definitions",
                Pattern = "api/service-configs/##/endpoint-definitions",
                Method = "get"
            });


            dbContext.ServiceConfigs.Update(serviceConfig);
            dbContext.SaveChanges();
        }
    }
    catch (Exception)
    {
        // ignored
    }
}


app.UseCors(b => b.AllowAnyHeader()
    .AllowAnyMethod()
    .SetIsOriginAllowed(_ => true)
    .AllowCredentials());

app.UseFastEndpoints(config => { config.Endpoints.RoutePrefix = "api"; });

await app.RunAsync();