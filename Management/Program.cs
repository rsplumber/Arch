using Core.Domains;
using Core.EndpointDefinitions;
using Core.PatternTree;
using Data.Sql;
using FastEndpoints;
using Management;
using Microsoft.EntityFrameworkCore;
using EndpointDefinition = Core.Domains.EndpointDefinition;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel();
builder.WebHost.UseUrls("http://+:5229");

builder.Services.AddSingleton<IEndpointDefinitionResolver, EndpointDefinitionResolver>();
builder.Services.AddSingleton<IEndpointPatternTree, InMemoryEndpointPatternTree>();
builder.Services.AddSingleton<IEndpointDefinitionContainer, InMemoryEndpointDefinitionContainer>();

builder.Services.AddSingleton<IEndpointPatternTree, InMemoryEndpointPatternTree>();

builder.Services.AddScoped<IEndpointDefinitionService, EndpointDefinitionService>();
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
                Name = "arch",
                BaseUrl = "http://localhost:5229"
            };
            dbContext.ServiceConfigs.Add(serviceConfig);
            dbContext.SaveChanges();
            if (dbContext.EndpointDefinitions.Any()) return;
            serviceConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "api/endpoint-definitions",
                Pattern = "api/endpoint-definitions"
            });
            serviceConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "api/endpoint-definitions/{id}",
                Pattern = "api/endpoint-definitions/##"
            });
            serviceConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "api/service-configs",
                Pattern = "api/service-configs"
            });
            serviceConfig.EndpointDefinitions.Add(new EndpointDefinition
            {
                Endpoint = "api/service-configs/{id}",
                Pattern = "api/service-configs/##"
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

app.UseFastEndpoints(config =>
{
    config.Endpoints.RoutePrefix = "api";
});

await app.RunAsync();