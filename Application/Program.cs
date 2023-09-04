using System.Text.Json;
using Arch;
using Arch.Authorization.Abstractions;
using Arch.Authorization.Kundera;
using Arch.Data.Caching.Abstractions;
using Arch.Data.Caching.InMemory;
using Arch.Data.EF;
using Arch.EndpointGraph.InMemory;
using Arch.LoadBalancer.Basic;
using Arch.Logging.Abstractions;
using Arch.Logging.Console;
using EventBus.Cap;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel(options => { options.Limits.MaxRequestBodySize = 50_000_000; });
builder.WebHost.ConfigureKestrel((_, options) => { options.ListenAnyIP(5228, _ => { }); });

builder.Services.AddArch(options =>
{
    options.EnableHealthCheck();
    options.EnableCors();
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

    options.ConfigureEndpointGraph(graphOptions => graphOptions.UseInMemory());
    options.ConfigureLoadBalancer(balancerOptions => balancerOptions.UseBasic());
    options.ConfigureData(dataOptions =>
    {
        dataOptions.UseEntityFramework(optionsBuilder => optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
        dataOptions.AddCaching(cachingOptions => cachingOptions.UseInMemory());
    });

    options.AddLogging(loggingOptions => loggingOptions.UseConsole());
    options.AddAuthorization(authorizationOptions => authorizationOptions.UseKundera(builder.Configuration));
});

var app = builder.Build();

app.UseArch(options =>
{
    options.UseData(dataOptions => dataOptions.UseEntityFramework());
    options.UseEndpointGraph(graphOptions => graphOptions.UseInMemory());
    options.BeforeDispatching(dispatchingOptions => dispatchingOptions.UseAuthorization(executionOptions => executionOptions.UseKundera(builder.Configuration)));
    options.AfterDispatching(dispatchingOptions => dispatchingOptions.UseLogging());
});
await app.RunAsync();