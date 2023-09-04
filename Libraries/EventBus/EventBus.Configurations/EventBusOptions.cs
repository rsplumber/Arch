using Microsoft.Extensions.DependencyInjection;

namespace Arch.EventBus.Configurations;

public sealed class EventBusOptions
{
    public IServiceCollection Services { get; init; } = default!;
}