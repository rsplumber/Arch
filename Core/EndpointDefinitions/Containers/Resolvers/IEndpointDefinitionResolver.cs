namespace Core.EndpointDefinitions.Containers.Resolvers;

public interface IEndpointDefinitionResolver
{
    ContainerEndpointDefinition? Resolve(string url, string method);

    ValueTask<ContainerEndpointDefinition?> ResolveAsync(string url, string method, CancellationToken cancellationToken = default);
}