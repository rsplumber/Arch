using System.Text.Json;
using Arch.Configurations;
using Arch.Core.Extensions;
using Arch.Data.Abstractions;
using Arch.EndpointGraph.Abstractions;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Arch;

public static class ApplicationBuilderExtension
{
    public static void UseArch(this IApplicationBuilder app, Action<ArchExecutionOptions> archOptions)
    {
        ArgumentNullException.ThrowIfNull(archOptions);

        app.Use(async (context, next) =>
        {
            context.Request.EnableBuffering();
            await next();
        });

        var archExecutionOptions = new ArchExecutionOptions
        {
            ApplicationBuilder = app
        };
        archOptions.Invoke(archExecutionOptions);

        ArgumentNullException.ThrowIfNull(archExecutionOptions.DataExecutionOptions);
        archExecutionOptions.DataExecutionOptions.Invoke(new DataExecutionOptions
        {
            ServiceProvider = app.ApplicationServices
        });

        ArgumentNullException.ThrowIfNull(archExecutionOptions.EndpointGraphExecutionOptions);
        archExecutionOptions.EndpointGraphExecutionOptions.Invoke(new EndpointGraphExecutionOptions
        {
            ServiceProvider = app.ApplicationServices
        });

        if (archExecutionOptions.CorsConfigurations is null)
        {
            app.UseCors(b => b.AllowAnyHeader()
                .AllowAnyMethod()
                .SetIsOriginAllowed(_ => true)
                .AllowCredentials());
        }

        app.UseCore(builder =>
        {
            if (archExecutionOptions.BeforeDispatchingOptions is null) return;
            archExecutionOptions.BeforeDispatchingOptions.Invoke(new BeforeDispatchingOptions
            {
                ApplicationBuilder = builder
            });
        }, builder =>
        {
            if (archExecutionOptions.AfterDispatchingOptions is null) return;
            archExecutionOptions.AfterDispatchingOptions.Invoke(new AfterDispatchingOptions
            {
                ApplicationBuilder = builder
            });
        });
        if (archExecutionOptions.HealthCheckEnabled)
        {
            app.UseHealthChecks(archExecutionOptions.HealthCheckUrl);
        }

        app.UseFastEndpoints(config =>
        {
            config.Serializer.Options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            config.Endpoints.RoutePrefix = "api";
            config.Versioning.Prefix = "v";
            config.Versioning.PrependToRoute = true;
        });
    }
}