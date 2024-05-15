using System.Text.Json;
using System.Text.Json.Serialization;
using Arch;
using Arch.Authorization.Abstractions;
using Arch.Authorization.Kundera;
using Arch.Core.ServiceConfigs;
using Arch.Data.Caching.Abstractions;
using Arch.Data.Caching.InMemory;
using Arch.Data.EF;
using Arch.EndpointGraph.InMemory;
using Arch.EventBus.Cap;
using Arch.LoadBalancer.Basic;
using Arch.Logging.Abstractions;
using Arch.Logging.Logstash;
using Encryption.Abstractions;
using Encryption.Tes.Security;
using Microsoft.EntityFrameworkCore;
using RateLimit.Cage;
using RateLimit.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel(options => { options.Limits.MaxRequestBodySize = 50_000_000; });
builder.WebHost.ConfigureKestrel((_, options) =>
{
    options.ListenAnyIP(5229, listenOptions =>
    {
        // listenOptions.UseHttps("wwwroot/cert/ssl_cert.pfx", "D!gi#b@nk1402");
    });
});

builder.Services.AddArch(options =>
{
    options.UseRateLimit(options => { options.AddCage(); });
    options.EnableHealthCheck();
    options.EnableCors();
    options.ConfigureEventBus(busOptions => busOptions.UseCap(capOptions =>
    {
        capOptions.FailedRetryCount = 0;
        capOptions.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        capOptions.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        capOptions.JsonSerializerOptions.WriteIndented = true;
        capOptions.JsonSerializerOptions.IgnoreReadOnlyFields = true;
        capOptions.SucceedMessageExpiredAfter = 60 * 2;
        capOptions.FailedMessageExpiredAfter = 60 * 2;
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

    options.AddLogging(loggingOptions => loggingOptions.UseLogstash());
    options.AddEncryption(encryptionOptions => encryptionOptions.UseTesSecurityEncryption(builder.Configuration));
    options.AddAuthorization(authorizationOptions => authorizationOptions.UseKundera(builder.Configuration));
});

var app = builder.Build();

app.UseArch(options =>
{
    options.UseData(dataOptions => dataOptions.UseEntityFramework());
    options.UseEndpointGraph(graphOptions =>
    {
        graphOptions.UseInMemory();
        using var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var serviceConfigRepository = serviceScope.ServiceProvider.GetRequiredService<IServiceConfigRepository>();
        var serviceConfigs = serviceConfigRepository.FindAsync().Result;
        var endpoints = (from config in serviceConfigs from definition in config.EndpointDefinitions select definition.Endpoint).ToList();
        graphOptions.InitializeWith(endpoints);
    });
    options.BeforeDispatching(dispatchingOptions =>
    {
        dispatchingOptions.UseRequestEncryption(executionOptions => executionOptions.UseTesSecurityEncryption());
        // dispatchingOptions.UseRateLimit(executionOptions => executionOptions.UseCage());
        dispatchingOptions.UseAuthorization(executionOptions => executionOptions.UseKundera(builder.Configuration));
    });
    options.AfterDispatching(dispatchingOptions =>
    {
        dispatchingOptions.UseLogging();
        dispatchingOptions.UseResponseEncryption(executionOptions => executionOptions.UseTesSecurityEncryption());
    });
});

await app.RunAsync();