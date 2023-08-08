namespace Core.Containers.Resolvers;

public sealed class EndpointDefinitionResolver : IEndpointDefinitionResolver
{
    private readonly IEndpointGraph _endpointPatternTree;
    private readonly IEndpointDefinitionContainer _endpointDefinitionContainer;

    public EndpointDefinitionResolver(IEndpointGraph endpointPatternTree, IEndpointDefinitionContainer endpointDefinitionContainer)
    {
        _endpointPatternTree = endpointPatternTree;
        _endpointDefinitionContainer = endpointDefinitionContainer;
    }

    public async ValueTask<(ContainerEndpointDefinition?, object[])> ResolveAsync(string url, HttpMethod method, CancellationToken cancellationToken = default)
    {
        var (pattern, pathParameters) = await _endpointPatternTree.FindAsync(url, cancellationToken);
        if (pattern is null) return (null, pathParameters);
        var containerEndpointDefinition = await _endpointDefinitionContainer.GetAsync(DefinitionKey.From(pattern, method), cancellationToken);
        return (containerEndpointDefinition, pathParameters);
    }
}