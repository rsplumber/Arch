using Arch.Core.ServiceConfigs.EndpointDefinitions.Events;
using Arch.Core.ServiceConfigs.Events;
using DotNetCore.CAP;

namespace Arch.Core.EndpointResolver;

internal sealed class RoutingGraphEventHandlers : ICapSubscribe
{
    private readonly IEndpointGraphResynchronizer _resynchronizer;

    public RoutingGraphEventHandlers(IEndpointGraphResynchronizer resynchronizer)
    {
        _resynchronizer = resynchronizer;
    }

    [CapSubscribe("arch.endpoint-definition.created")]
    public Task EndpointCreatedAsync(EndpointDefinitionCreatedEvent message, CancellationToken cancellationToken = default)
        => _resynchronizer.ResyncAsync(cancellationToken).AsTask();

    [CapSubscribe("arch.endpoint-definition.changed")]
    public Task EndpointChangedAsync(EndpointDefinitionChangedEvent message, CancellationToken cancellationToken = default)
        => _resynchronizer.ResyncAsync(cancellationToken).AsTask();

    [CapSubscribe("arch.endpoint-definition.removed")]
    public Task EndpointRemovedAsync(EndpointDefinitionRemovedEvent message, CancellationToken cancellationToken = default)
        => _resynchronizer.ResyncAsync(cancellationToken).AsTask();

    [CapSubscribe("arch.service-config.created")]
    public Task ServiceCreatedAsync(ServiceConfigCreatedEvent message, CancellationToken cancellationToken = default)
        => _resynchronizer.ResyncAsync(cancellationToken).AsTask();

    [CapSubscribe("arch.service-config.changed")]
    public Task ServiceChangedAsync(ServiceConfigChangedEvent message, CancellationToken cancellationToken = default)
        => _resynchronizer.ResyncAsync(cancellationToken).AsTask();

    [CapSubscribe("arch.service-config.removed")]
    public Task ServiceRemovedAsync(ServiceConfigRemovedEvent message, CancellationToken cancellationToken = default)
        => _resynchronizer.ResyncAsync(cancellationToken).AsTask();
}
