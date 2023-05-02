using System.Text.Json;
using Application.Middlewares;
using Arch.Clerk;
using Arch.Kundera;
using Core;
using Core.Entities.EndpointDefinitions.Containers.Resolvers;
using Core.Entities.EndpointDefinitions.Services;
using Core.Entities.ServiceConfigs.Services;
using Data.InMemory;
using Data.Sql;
using FastEndpoints;

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
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<ExceptionHandlerMiddleware>();
builder.Services.AddCore(collection =>
{
    collection.AddKundera(builder.Configuration);
    collection.AddClerkAccounting(builder.Configuration);
});

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
var app = builder.Build();

app.UseCors(b => b.AllowAnyHeader()
    .AllowAnyMethod()
    .SetIsOriginAllowed(_ => true)
    .AllowCredentials());


app.UseHealthChecks("/health");
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    await next();
});
app.UseData(builder.Configuration);
app.UseCore(applicationBuilder =>
{
    applicationBuilder.UseKundera(builder.Configuration);
    applicationBuilder.UseClerkAccounting(builder.Configuration);
}, applicationBuilder =>
{
    // applicationBuilder.UseAllElasticApm(builder.Configuration);
});
app.UseInMemoryData(builder.Configuration);


app.UseFastEndpoints(config =>
{
    config.Serializer.Options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    config.Endpoints.RoutePrefix = "gateway/api";
    config.Versioning.Prefix = "v";
    config.Versioning.PrependToRoute = true;
});


await app.RunAsync();