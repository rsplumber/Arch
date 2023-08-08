using Core;
using Core.EndpointDefinitions;
using Core.ServiceConfigs;

namespace Data.InMemory;

internal sealed class InMemoryContainerInitializer
{
    private readonly IEndpointGraph _endpointPatternTree;
    private readonly IEndpointDefinitionContainer _endpointDefinitionContainer;

    public InMemoryContainerInitializer(IEndpointGraph endpointPatternTree, IEndpointDefinitionContainer endpointDefinitionContainer)
    {
        _endpointPatternTree = endpointPatternTree;
        _endpointDefinitionContainer = endpointDefinitionContainer;
    }

    public async Task InitializeAsync(List<ServiceConfig> serviceConfigs, CancellationToken cancellationToken = default)
    {
        await _endpointPatternTree.ClearAsync(cancellationToken);
        await _endpointDefinitionContainer.ClearAsync(cancellationToken);
        foreach (var config in serviceConfigs)
        {
            foreach (var definition in config.EndpointDefinitions.Where(definition => !definition.IsDisabled()))
            {
                definition.Meta.AddRange(config.Meta);
                await _endpointPatternTree.AddAsync(definition.Endpoint, cancellationToken);
                await _endpointDefinitionContainer.AddAsync(definition, cancellationToken);
            }
        }
    }
}