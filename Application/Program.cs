using Application.Dispatcher;
using Core.EndpointDefinitions;
using Core.PatternTree;
using Core.RequestDispatcher;
using Data.Sql;
using FastEndpoints;
using FastEndpoints.Swagger;
using Management;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel();
builder.WebHost.UseUrls("http://+:5228");

builder.Services.AddHttpClient("default", _ => { });

builder.Services.AddSingleton<IEndpointDefinitionResolver, EndpointDefinitionResolver>();
builder.Services.AddSingleton<IEndpointPatternTree, InMemoryEndpointPatternTree>();
builder.Services.AddSingleton<IEndpointDefinitionContainer, InMemoryEndpointDefinitionContainer>();

builder.Services.AddSingleton<IEndpointPatternTree, InMemoryEndpointPatternTree>();

builder.Services.AddScoped<IEndpointDefinitionService, EndpointDefinitionService>();

builder.Services.AddScoped<IRequestDispatcher, RequestDispatcher>();

builder.Services.AddData(builder.Configuration);
builder.Services.AddManagement(builder.Configuration);
builder.Services.AddCors();

builder.Services.AddFastEndpoints();
builder.Services.AddSwaggerDoc(settings =>
{
    settings.Title = "Arch - WebApi";
    settings.DocumentName = "v1";
    settings.Version = "v1";
}, addJWTBearerAuth: false, maxEndpointVersion: 1);
var app = builder.Build();
app.UseData();
await InitializeInMemoryContainers();
app.UseCors(b => b.AllowAnyHeader()
    .AllowAnyMethod()
    .SetIsOriginAllowed(_ => true)
    .AllowCredentials());

app.UseFastEndpoints(config => { config.Endpoints.RoutePrefix = "api"; });

// if (app.Environment.IsDevelopment())
// {
app.UseOpenApi();
app.UseSwaggerUi3(s => s.ConfigureDefaults());
// }


await app.RunAsync();

async Task InitializeInMemoryContainers()
{
    using var serviceScope = app.Services.GetService<IServiceScopeFactory>()?.CreateScope();
    var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
    var tree = serviceScope.ServiceProvider.GetRequiredService<IEndpointPatternTree>();
    var endpointDefinitionContainer = serviceScope.ServiceProvider.GetRequiredService<IEndpointDefinitionContainer>();
    var configs = await dbContext.ServiceConfigs.Include(config => config.EndpointDefinitions)
        .ToListAsync();
    foreach (var serviceConfigEndpointDefinition in configs.SelectMany(serviceConfig => serviceConfig.EndpointDefinitions))
    {
        tree.Add(serviceConfigEndpointDefinition.Endpoint);
        tree.Find(serviceConfigEndpointDefinition.Endpoint);
        await endpointDefinitionContainer.AddAsync(serviceConfigEndpointDefinition);
    }
}