namespace Core.EndpointDefinitions.Resolvers;

public interface IEndpointDefinitionResolver
{
    EndpointDefinition? Resolve(string url, string method);

    ValueTask<EndpointDefinition?> ResolveAsync(string url, string method, CancellationToken cancellationToken = default);
}