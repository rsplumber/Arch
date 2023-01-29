using Application.Middlewares;
using Arch.Clerk;
using Arch.Kundera;
using Core.EndpointDefinitions.Containers;
using Core.EndpointDefinitions.Resolvers;
using Core.EndpointDefinitions.Services;
using Core.ServiceConfigs.Services;
using Data.Sql;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel();
builder.WebHost.UseUrls("http://+:5228");

builder.Services.AddHttpClient("default", _ => { });

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