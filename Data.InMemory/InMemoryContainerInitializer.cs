using Core.EndpointDefinitions.Containers;
using Core.ServiceConfigs;

namespace Data.InMemory;

internal sealed class InMemoryContainerInitializer : IContainerInitializer
{
    private readonly IEndpointPatternTree _endpointPatternTree;
    private readonly IEndpointDefinitionContainer _endpointDefinitionContainer;

    public InMemoryContainerInitializer(IEndpointPatternTree endpointPatternTree, IEndpointDefinitionContainer endpointDefinitionContainer)
    {
        _endpointPatternTree = endpointPatternTree;
        _endpointDefinitionContainer = endpointDefinitionContainer;
    }

    public async Task InitializeAsync(List<ServiceConfig> serviceConfigs, CancellationToken cancellationToken = default)
    {
        _endpointPatternTree.Clear();
        _endpointDefinitionContainer.Clear();
        foreach (var config in serviceConfigs)
        {
            foreach (var definition in config.EndpointDefinitions)
            {
                definition.Meta.AddRange(config.Meta);
                _endpointPatternTree.Add(definition.Endpoint);
                await _endpointDefinitionContainer.AddAsync(definition, cancellationToken);
            }
        }
    }
}