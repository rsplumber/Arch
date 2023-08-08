namespace Core.EndpointDefinitions;

public interface IEndpointDefinitionResolver
{
    ValueTask<(ContainerEndpointDefinition?, object[])> ResolveAsync(string url, HttpMethod method, CancellationToken cancellationToken = default);
}