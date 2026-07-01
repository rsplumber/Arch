using System.Text.Json;
using System.Text.Json.Serialization;
using Arch;
using Arch.Admin.Hosting;
using Arch.Authorization.Abstractions;
using Arch.Authorization.Kundera;
using Arch.Authorization.SimpleJwt;
using Arch.Core.ServiceConfigs;
using Arch.Data.Caching.Abstractions;
using Arch.Data.Caching.InMemory;
using Arch.Data.EF;
using Arch.EndpointResolver.Graph.InMemory;
using Arch.EventBus.Cap;
using Arch.LoadBalancer.Basic;
using Arch.Logging.Abstractions;
using Arch.Logging.Logstash;
using Encryption.Abstractions;
using Encryption.Archrypt;
using Microsoft.EntityFrameworkCore;
using RateLimit.ArchLimit;
using RateLimit.Configuration;
using Savorboard.CAP.InMemoryMessageQueue;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel(options => { options.Limits.MaxRequestBodySize = 50_000_000; });
builder.WebHost.ConfigureKestrel((_, options) =>
{
    options.ListenAnyIP(5229, listenOptions =>
    {
        // listenOptions.UseHttps("wwwroot/cert/*.pfx", "your_password");
    });
});

builder.Services.AddArch(options =>
{
    options.UseRateLimit(options => { options.AddArchLimit(s => s.UseInMemoryStore()); });
    options.EnableHealthCheck();
    options.EnableCors();
    options.ConfigureEventBus(busOptions => busOptions.UseCap(capOptions =>
    {
        capOptions.FailedRetryCount = 0;
        // Per-instance consumer group: subscribers that omit an explicit Group (the endpoint/service
        // cache+tree invalidation handlers) fall back to this, giving each instance its own queue so
        // every instance receives every invalidation (broadcast fan-out) — required for zero-downtime
        // config changes across a horizontally-scaled fleet. Subscribers that must stay
        // competing-consumer (e.g. logging) pin their own fixed Group and are unaffected.
        //
        // The group is keyed by a STABLE instance id (configured "InstanceId", else the host/pod name)
        // rather than a per-process GUID, so a restart reuses the same queue instead of orphaning the
        // old one. Set InstanceId explicitly (e.g. to the pod name) when running multiple instances on
        // one host. As defence against queues left by permanently-removed instances (scale-down), the
        // RabbitMQ queues are also given an idle auto-expire below.
        var instanceId = builder.Configuration.GetValue<string>("InstanceId") ?? Environment.MachineName;
        capOptions.DefaultGroupName = $"arch.core.{instanceId}";
        capOptions.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        capOptions.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        capOptions.JsonSerializerOptions.WriteIndented = true;
        capOptions.JsonSerializerOptions.IgnoreReadOnlyFields = true;
        capOptions.SucceedMessageExpiredAfter = 60 * 2;
        capOptions.FailedMessageExpiredAfter = 60 * 2;
        capOptions.UseInMemoryMessageQueue();
        // capOptions.UseRabbitMQ(op =>
        // {
        //     op.HostName = builder.Configuration.GetValue<string>("RabbitMQ:HostName") ?? throw new ArgumentNullException("RabbitMQ:HostName", "Enter RabbitMQ:HostName in app settings");
        //     op.UserName = builder.Configuration.GetValue<string>("RabbitMQ:UserName") ?? throw new ArgumentNullException("RabbitMQ:UserName", "Enter RabbitMQ:UserName in app settings");
        //     op.Password = builder.Configuration.GetValue<string>("RabbitMQ:Password") ?? throw new ArgumentNullException("RabbitMQ:Password", "Enter RabbitMQ:UserName in app settings");
        //     op.ExchangeName = builder.Configuration.GetValue<string>("RabbitMQ:ExchangeName") ?? throw new ArgumentNullException("RabbitMQ:ExchangeName", "Enter RabbitMQ:ExchangeName in app settings");
        //     // Auto-delete a queue once it has had no consumer for this long (x-expires). Active
        //     // instances always have a consumer, so only queues left by removed instances are reaped.
        //     op.QueueArguments.QueueMessageExpires = (int)TimeSpan.FromHours(1).TotalMilliseconds;
        // });
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
    options.AddEncryption(encryptionOptions => encryptionOptions.UseArchrypt());
    options.AddAuthorization(authorizationOptions => authorizationOptions.UseSimpleJwt());
});

// Embedded admin panel (Blazor Server). Mounted as an isolated /admin branch below.
builder.Services.AddArchAdmin();

var app = builder.Build();

// Mount the admin panel BEFORE UseArch: the gateway's request-extractor returns 404 for any
// path that isn't a registered upstream endpoint, so /admin must be branched off first.
app.MapArchAdmin();

using var serviceScope = app.Services.GetService<IServiceScopeFactory>()?.CreateScope();
if (serviceScope == null) return;


app.UseArch(options =>
{
    options.UseData(dataOptions => dataOptions.UseEntityFramework());
    options.BeforeDispatching(dispatchingOptions =>
    {
        dispatchingOptions.UseAuthorization(executionOptions => executionOptions.UseSimpleJwt(builder.Configuration));
        dispatchingOptions.UseRequestEncryption();
        dispatchingOptions.UseRateLimit(executionOptions => executionOptions.UseArchLimit(builder.Configuration));
    });
    options.AfterDispatching(dispatchingOptions =>
    {
        dispatchingOptions.UseLogging();
        dispatchingOptions.UseResponseEncryption();
    });
});

await app.RunAsync();