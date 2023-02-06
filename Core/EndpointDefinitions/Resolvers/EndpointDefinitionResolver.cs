using Core.EndpointDefinitions.Containers;

namespace Core.EndpointDefinitions.Resolvers;

public sealed class EndpointDefinitionResolver : IEndpointDefinitionResolver
{
    private readonly IEndpointPatternTree _endpointPatternTree;
    private readonly IEndpointDefinitionContainer _endpointDefinitionContainer;

    public EndpointDefinitionResolver(IEndpointPatternTree endpointPatternTree, IEndpointDefinitionContainer endpointDefinitionContainer)
    {
        _endpointPatternTree = endpointPatternTree;
        _endpointDefinitionContainer = endpointDefinitionContainer;
    }

    public EndpointDefinition? Resolve(string url, string method)
    {
        var pattern = _endpointPatternTree.Find(url);
        return _endpointDefinitionContainer.Get(DefinitionKey.From(pattern, method));
    }

    public async ValueTask<EndpointDefinition?> ResolveAsync(string url, string method, CancellationToken cancellationToken = default)
    {
        var pattern = await _endpointPatternTree.FindAsync(url, cancellationToken);
        return await _endpointDefinitionContainer.GetAsync(DefinitionKey.From(pattern, method), cancellationToken);
    }
}