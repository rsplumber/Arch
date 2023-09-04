using Microsoft.Extensions.DependencyInjection;

namespace EventBus.Configurations;

public sealed class EventBusOptions
{
    public IServiceCollection Services { get; init; } = default!;
}