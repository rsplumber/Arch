using System.Text.Json;
using Arch.Kundera;
using Caching.Abstractions;
using Caching.InMemory;
using Core.Extensions;
using Data.Abstractions;
using Data.EFCore;
using Elastic.Apm.NetCoreAll;
using EndpointGraph.Abstractions;
using EndpointGraph.InMemory;
using FastEndpoints;
using Logging.Logstash;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel(options => { options.Limits.MaxRequestBodySize = 50_000_000; });
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
builder.Services.AddCore(options => { options.AddKundera(builder.Configuration); });


builder.Services.AddLoggingLogstash();
builder.Services.AddCap(options =>
{
    options.FailedRetryCount = 2;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.IgnoreReadOnlyFields = true;
    options.SucceedMessageExpiredAfter = 1;
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

builder.Services.AddData(options =>
{
    options.UseEntityFramework(optionsBuilder => optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
    options.AddCaching(cachingOptions => cachingOptions.UseInMemory());
});
builder.Services.AddEndpointGraph(options => { options.UseInMemoryEndpointGraph(); });

builder.Services.AddFastEndpoints();
var app = builder.Build();

app.UseCors(b => b.AllowAnyHeader()
    .AllowAnyMethod()
    .SetIsOriginAllowed(_ => true)
    .AllowCredentials());


app.UseHealthChecks("/health");
app.Services.UseData(options => { options.UseEntityFramework(); });
app.UseCore(applicationBuilder => { applicationBuilder.UseKundera(builder.Configuration); }, applicationBuilder => { applicationBuilder.UseAllElasticApm(builder.Configuration); });

app.Services.UseEndpointGraph(options => { options.UseInMemory(); });

app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    await next();
});
app.UseFastEndpoints(config =>
{
    config.Serializer.Options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    config.Endpoints.RoutePrefix = "gateway/api";
    config.Versioning.Prefix = "v";
    config.Versioning.PrependToRoute = true;
});


await app.RunAsync();