using Core.EndpointDefinitions.Events;
using Core.ServiceConfigs;
using DotNetCore.CAP;

namespace Data.InMemory;

internal sealed class EndpointDefinitionEventHandlers : ICapSubscribe
{
    private readonly IServiceConfigRepository _serviceConfigRepository;
    private readonly InMemoryContainerInitializer _containerInitializer;

    public EndpointDefinitionEventHandlers(IServiceConfigRepository serviceConfigRepository, InMemoryContainerInitializer containerInitializer)
    {
        _serviceConfigRepository = serviceConfigRepository;
        _containerInitializer = containerInitializer;
    }

    [CapSubscribe("arch.endpoint-definition.created", Group = "arch.core.queue")]
    public Task EndpointCreatedAsync(EndpointDefinitionCreatedEvent message, CancellationToken cancellationToken = default)
    {
        return ReInitializeContainersAsync(cancellationToken);
    }

    [CapSubscribe("arch.endpoint-definition.changed", Group = "arch.core.queue")]
    public Task EndpointCreatedAsync(EndpointDefinitionChangedEvent message, CancellationToken cancellationToken = default)
    {
        return ReInitializeContainersAsync(cancellationToken);
    }

    [CapSubscribe("arch.endpoint-definition.removed", Group = "arch.core.queue")]
    public Task EndpointCreatedAsync(EndpointDefinitionRemovedEvent message, CancellationToken cancellationToken = default)
    {
        return ReInitializeContainersAsync(cancellationToken);
    }

    private async Task ReInitializeContainersAsync(CancellationToken cancellationToken = default)
    {
        var serviceConfigs = await _serviceConfigRepository.FindAsync(cancellationToken);
        await _containerInitializer.InitializeAsync(serviceConfigs, cancellationToken);
    }
}