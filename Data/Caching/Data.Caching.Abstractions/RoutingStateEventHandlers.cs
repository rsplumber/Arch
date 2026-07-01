using Arch.Core.ServiceConfigs;
using Arch.Core.ServiceConfigs.EndpointDefinitions;
using Arch.Core.ServiceConfigs.EndpointDefinitions.Events;
using Arch.Core.ServiceConfigs.Events;
using DotNetCore.CAP;

namespace Arch.Data.Caching.Abstractions;

internal sealed class RoutingStateEventHandlers : ICapSubscribe
{
    private readonly IEndpointDefinitionCache _cache;
    private readonly IEndpointDefinitionRepository _endpointDefinitionRepository;
    private readonly IServiceConfigRepository _serviceConfigRepository;

    public RoutingStateEventHandlers(
        IEndpointDefinitionCache cache,
        IEndpointDefinitionRepository endpointDefinitionRepository,
        IServiceConfigRepository serviceConfigRepository)
    {
        _cache = cache;
        _endpointDefinitionRepository = endpointDefinitionRepository;
        _serviceConfigRepository = serviceConfigRepository;
    }

    [CapSubscribe("arch.endpoint-definition.changed", Group = "arch.core.queue")]
    public async Task EndpointChangedAsync(EndpointDefinitionChangedEvent message, CancellationToken cancellationToken = default)
    {
        var definition = await _endpointDefinitionRepository.FindAsync(message.Id, cancellationToken);
        if (definition is null) return;
        _cache.Remove(DefinitionKey.From(definition.Pattern, definition.Method));
    }

    [CapSubscribe("arch.endpoint-definition.removed", Group = "arch.core.queue")]
    public Task EndpointRemovedAsync(EndpointDefinitionRemovedEvent message, CancellationToken cancellationToken = default)
    {
        _cache.Remove(DefinitionKey.From(message.Pattern, new HttpMethod(message.Method)));
        return Task.CompletedTask;
    }

    [CapSubscribe("arch.service-config.changed", Group = "arch.core.queue")]
    public async Task ServiceChangedAsync(ServiceConfigChangedEvent message, CancellationToken cancellationToken = default)
    {
        var serviceConfig = await _serviceConfigRepository.FindAsync(message.Id, cancellationToken);
        if (serviceConfig is null) return;
        foreach (var definition in serviceConfig.EndpointDefinitions)
        {
            _cache.Remove(DefinitionKey.From(definition.Pattern, definition.Method));
        }
    }

    [CapSubscribe("arch.service-config.removed", Group = "arch.core.queue")]
    public Task ServiceRemovedAsync(ServiceConfigRemovedEvent message, CancellationToken cancellationToken = default)
    {
        foreach (var endpoint in message.Endpoints)
        {
            _cache.Remove(DefinitionKey.From(endpoint.Pattern, new HttpMethod(endpoint.Method)));
        }
        return Task.CompletedTask;
    }
}
