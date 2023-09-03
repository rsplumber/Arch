using Arch.Core.EndpointDefinitions;
using Arch.Core.EndpointDefinitions.Events;
using Arch.Core.ServiceConfigs;
using DotNetCore.CAP;

namespace Arch.Data.Caching.InMemory;

internal sealed class EventHandlers : ICapSubscribe
{
    private readonly InMemoryEndpointDefinitionResolver _inMemoryEndpointDefinitionResolver;
    private readonly IEndpointDefinitionRepository _endpointDefinitionRepository;
    private readonly IServiceConfigRepository _serviceConfigRepository;

    public EventHandlers(InMemoryEndpointDefinitionResolver inMemoryEndpointDefinitionResolver, IEndpointDefinitionRepository endpointDefinitionRepository, IServiceConfigRepository serviceConfigRepository)
    {
        _inMemoryEndpointDefinitionResolver = inMemoryEndpointDefinitionResolver;
        _endpointDefinitionRepository = endpointDefinitionRepository;
        _serviceConfigRepository = serviceConfigRepository;
    }

    [CapSubscribe("arch.endpoint-definition.created", Group = "arch.core.queue")]
    public async Task EndpointCreatedAsync(EndpointDefinitionCreatedEvent message, CancellationToken cancellationToken = default)
    {
        var createdDefinition = await _endpointDefinitionRepository.FindAsync(message.Id, cancellationToken);
        if (createdDefinition is null) return;
        _inMemoryEndpointDefinitionResolver.EndpointDefinitionsContainer.TryAdd(DefinitionKey.From(createdDefinition.Pattern, createdDefinition.Method), createdDefinition);
    }

    [CapSubscribe("arch.endpoint-definition.changed", Group = "arch.core.queue")]
    public async Task EndpointChangedAsync(EndpointDefinitionChangedEvent message, CancellationToken cancellationToken = default)
    {
        var changedDefinition = await _endpointDefinitionRepository.FindAsync(message.Id, cancellationToken);
        if (changedDefinition is null) return;
        var definitionKey = DefinitionKey.From(changedDefinition.Pattern, changedDefinition.Method);
        _inMemoryEndpointDefinitionResolver.EndpointDefinitionsContainer.TryRemove(definitionKey, out var _);
        _inMemoryEndpointDefinitionResolver.EndpointDefinitionsContainer.TryAdd(DefinitionKey.From(changedDefinition.Pattern, changedDefinition.Method), changedDefinition);
    }

    [CapSubscribe("arch.endpoint-definition.removed", Group = "arch.core.queue")]
    public async Task EndpointRemovedAsync(EndpointDefinitionRemovedEvent message, CancellationToken cancellationToken = default)
    {
        var changedDefinition = await _endpointDefinitionRepository.FindAsync(message.Id, cancellationToken);
        if (changedDefinition is null) return;
        _inMemoryEndpointDefinitionResolver.EndpointDefinitionsContainer.TryRemove(DefinitionKey.From(changedDefinition.Pattern, changedDefinition.Method), out var _);
    }

    [CapSubscribe("arch.service-config.changed", Group = "arch.core.queue")]
    public async Task ServiceChangedAsync(EndpointDefinitionCreatedEvent message, CancellationToken cancellationToken = default)
    {
        var serviceConfig = await _serviceConfigRepository.FindAsync(message.Id, cancellationToken);
        serviceConfig?.EndpointDefinitions.ForEach(definition => { _inMemoryEndpointDefinitionResolver.EndpointDefinitionsContainer.TryRemove(DefinitionKey.From(definition.Pattern, definition.Method), out var _); });
    }

    [CapSubscribe("arch.service-config.removed", Group = "arch.core.queue")]
    public async Task ServiceRemovedAsync(EndpointDefinitionCreatedEvent message, CancellationToken cancellationToken = default)
    {
        var serviceConfig = await _serviceConfigRepository.FindAsync(message.Id, cancellationToken);
        serviceConfig?.EndpointDefinitions.ForEach(definition => { _inMemoryEndpointDefinitionResolver.EndpointDefinitionsContainer.TryRemove(DefinitionKey.From(definition.Pattern, definition.Method), out var _); });
    }
}