using System.Text.Json;
using Arch;
using Arch.Authorization.Abstractions;
using Arch.Data.Abstractions;
using Arch.Data.Caching.Abstractions;
using Arch.Data.Caching.InMemory;
using Arch.Data.EF;
using Arch.EndpointGraph.Abstractions;
using Arch.EndpointGraph.InMemory;
using Arch.Kundera;
using Arch.LoadBalancer.Basic;
using Arch.LoadBalancer.Configurations;
using Arch.Logging.Abstractions;
using Arch.Logging.Console;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(options => { options.Limits.MaxRequestBodySize = 50_000_000; });
builder.WebHost.ConfigureKestrel((_, options) => { options.ListenAnyIP(5228, _ => { }); });

builder.Services.AddArch(options =>
{
    options.ConfigureEventBus(busOptions => busOptions.UseCap(capOptions =>
    {
        capOptions.FailedRetryCount = 2;
        capOptions.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        capOptions.JsonSerializerOptions.IgnoreReadOnlyFields = true;
        capOptions.SucceedMessageExpiredAfter = 1;
        capOptions.UseRabbitMQ(op =>
        {
            op.HostName = builder.Configuration.GetValue<string>("RabbitMQ:HostName") ?? throw new ArgumentNullException("RabbitMQ:HostName", "Enter RabbitMQ:HostName in app settings");
            op.UserName = builder.Configuration.GetValue<string>("RabbitMQ:UserName") ?? throw new ArgumentNullException("RabbitMQ:UserName", "Enter RabbitMQ:UserName in app settings");
            op.Password = builder.Configuration.GetValue<string>("RabbitMQ:Password") ?? throw new ArgumentNullException("RabbitMQ:Password", "Enter RabbitMQ:UserName in app settings");
            op.ExchangeName = builder.Configuration.GetValue<string>("RabbitMQ:ExchangeName") ?? throw new ArgumentNullException("RabbitMQ:ExchangeName", "Enter RabbitMQ:ExchangeName in app settings");
        });
        capOptions.UsePostgreSql(sqlOptions =>
        {
            sqlOptions.ConnectionString = builder.Configuration.GetConnectionString("default") ?? throw new ArgumentNullException("connectionString", "Enter connection string in app settings");
            sqlOptions.Schema = "events";
        });
    }));

    options.ConfigureEndpointGraph(graphOptions => { graphOptions.UseInMemory(); });
    options.ConfigureLoadBalancer(balancerOptions => balancerOptions.UseBasic());
    options.ConfigureData(dataOptions =>
    {
        dataOptions.UseEntityFramework(optionsBuilder => optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
        dataOptions.AddCaching(cachingOptions => cachingOptions.UseInMemory());
    });

    options.AddLogging(loggingOptions => loggingOptions.UseConsole());
    options.AddAuthorization(authorizationOptions => authorizationOptions.UseKundera(builder.Configuration));
});

builder.Services.AddCors();
builder.Services.AddHealthChecks();
builder.Services.AddResponseCompression();
builder.Services.AddFastEndpoints();

var app = builder.Build();

app.UseCors(b => b.AllowAnyHeader()
    .AllowAnyMethod()
    .SetIsOriginAllowed(_ => true)
    .AllowCredentials());

app.UseArch(options =>
{
    options.UseData(dataOptions => { dataOptions.UseEntityFramework(); });
    options.UseEndpointGraph(graphOptions => { graphOptions.UseInMemory(); });
}, beforeDispatchOptions =>
{
    beforeDispatchOptions.UseAuthorization(options => options.UseKundera(builder.Configuration));
}, afterDispatchOptions => { });

app.UseHealthChecks("/health");
app.UseResponseCompression();
app.UseFastEndpoints(config =>
{
    config.Serializer.Options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    config.Endpoints.RoutePrefix = "api";
    config.Versioning.Prefix = "v";
    config.Versioning.PrependToRoute = true;
});

await app.RunAsync();