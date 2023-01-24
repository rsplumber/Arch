using Application.Dispatcher;
using Application.Middlewares;
using Core.Domains;
using Core.EndpointDefinitions;
using Core.PatternTree;
using Core.RequestDispatcher;
using Data.Sql;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel();
builder.WebHost.UseUrls("http://+:5228");

builder.Services.AddHttpClient("default", _ => { });
builder.Services.AddSingleton<RequestExtractorMiddleware>();
builder.Services.AddSingleton<RequestDispatcherMiddleware>();

builder.Services.AddSingleton<IEndpointDefinitionResolver, EndpointDefinitionResolver>();
builder.Services.AddSingleton<IEndpointPatternTree, InMemoryEndpointPatternTree>();
builder.Services.AddSingleton<IEndpointDefinitionContainer, InMemoryEndpointDefinitionContainer>();

builder.Services.AddSingleton<IEndpointPatternTree, InMemoryEndpointPatternTree>();

builder.Services.AddScoped<IEndpointDefinitionService, EndpointDefinitionService>();

builder.Services.AddSingleton<IRequestDispatcher, RequestDispatcher>();

builder.Services.AddData(builder.Configuration);
builder.Services.AddCors();

builder.Services.AddFastEndpoints();

var app = builder.Build();

app.UseMiddleware<RequestExtractorMiddleware>();
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
    var endpointPatternTree = serviceScope.ServiceProvider.GetRequiredService<IEndpointPatternTree>();
    var endpointDefinitionContainer = serviceScope.ServiceProvider.GetRequiredService<IEndpointDefinitionContainer>();
    var configs = await dbContext.ServiceConfigs.Include(config => config.EndpointDefinitions)
        .ToListAsync();
    configs.ForEach(config =>
    {
        config.EndpointDefinitions.ForEach(async definition =>
        {
            definition.Meta.Add(new Meta
            {
                Id = "base_url",
                Value = config.BaseUrl
            });
            endpointPatternTree.Add(definition.Endpoint);
            endpointPatternTree.Find(definition.Endpoint);
            await endpointDefinitionContainer.AddAsync(definition);
        });
    });
}