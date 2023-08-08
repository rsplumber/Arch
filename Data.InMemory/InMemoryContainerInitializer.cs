using Core;
using Core.Containers;
using Core.Entities.ServiceConfigs;

namespace Data.InMemory;

internal sealed class InMemoryContainerInitializer : IContainerInitializer
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
        _endpointPatternTree.ClearAsync();
        _endpointDefinitionContainer.Clear();
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