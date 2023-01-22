using Core.Domains;
using Core.PatternTree;

namespace Core.EndpointDefinitions;

public class EndpointDefinitionResolver : IEndpointDefinitionResolver
{
    private readonly IEndpointPatternTree _endpointPatternTree;
    private readonly IEndpointDefinitionContainer _endpointDefinitionContainer;

    public EndpointDefinitionResolver(IEndpointPatternTree endpointPatternTree, IEndpointDefinitionContainer endpointDefinitionContainer)
    {
        _endpointPatternTree = endpointPatternTree;
        _endpointDefinitionContainer = endpointDefinitionContainer;
    }

    public EndpointDefinition? Resolve(string url)
    {
        var pattern = _endpointPatternTree.Find(url);
        return _endpointDefinitionContainer.Get(pattern);
    }

    public async ValueTask<EndpointDefinition?> ResolveAsync(string url, CancellationToken cancellationToken = default)
    {
        var pattern = await _endpointPatternTree.FindAsync(url, cancellationToken);
        return await _endpointDefinitionContainer.GetAsync(pattern, cancellationToken);
    }
}