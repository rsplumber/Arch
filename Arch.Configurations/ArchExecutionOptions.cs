using Arch.Data.Abstractions;
using Arch.EndpointGraph.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Arch.Configurations;

public sealed class ArchExecutionOptions
{
    public IApplicationBuilder ApplicationBuilder { get; init; } = default!;

    internal Action<DataExecutionOptions> DataExecutionOptions { get; private set; } = default!;

    internal Action<EndpointGraphExecutionOptions> EndpointGraphExecutionOptions { get; private set; } = default!;

    internal Action<BeforeDispatchingOptions>? BeforeDispatchingOptions { get; private set; }

    internal Action<AfterDispatchingOptions>? AfterDispatchingOptions { get; private set; }

    internal Action<CorsPolicyBuilder>? CorsConfigurations { get; private set; }

    internal bool HealthCheckEnabled { get; private set; }

    internal string HealthCheckUrl { get; private set; } = default!;

    public void UseData(Action<DataExecutionOptions> dataExecutionOptions) => DataExecutionOptions = dataExecutionOptions;

    public void UseEndpointGraph(Action<EndpointGraphExecutionOptions> endpointGraphExecutionOptions) => EndpointGraphExecutionOptions = endpointGraphExecutionOptions;

    public void BeforeDispatching(Action<BeforeDispatchingOptions> beforeDispatchingOptions) => BeforeDispatchingOptions = beforeDispatchingOptions;

    public void AfterDispatching(Action<AfterDispatchingOptions> afterDispatchingOptions) => AfterDispatchingOptions = afterDispatchingOptions;

    public void EnableHealthCheck(string? url = null)
    {
        HealthCheckUrl = url ?? "health";
        HealthCheckEnabled = true;
    }

    public void ConfigureCors(Action<CorsPolicyBuilder> corsConfigurations) => CorsConfigurations = corsConfigurations;
}