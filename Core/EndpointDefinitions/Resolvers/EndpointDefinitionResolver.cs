using EndpointGraph.Abstractions;

namespace Core.EndpointDefinitions.Resolvers;

public sealed class EndpointDefinitionResolver : IEndpointDefinitionResolver
{
    private readonly IEndpointGraph _endpointPatternTree;
    private readonly IEndpointDefinitionRepository _endpointDefinitionRepository;

    public EndpointDefinitionResolver(IEndpointGraph endpointPatternTree, IEndpointDefinitionRepository endpointDefinitionRepository)
    {
        _endpointPatternTree = endpointPatternTree;
        _endpointDefinitionRepository = endpointDefinitionRepository;
    }


    public async ValueTask<(EndpointDefinition?, object[])> ResolveAsync(string url, HttpMethod method, CancellationToken cancellationToken = default)
    {
        var (pattern, pathParameters) = await _endpointPatternTree.FindAsync(url, cancellationToken);
        if (pattern is null) return (null, pathParameters);
        var endpointDefinition = await _endpointDefinitionRepository.FindAsync(DefinitionKey.From(pattern, method), cancellationToken);
        return (endpointDefinition, pathParameters);
    }
}