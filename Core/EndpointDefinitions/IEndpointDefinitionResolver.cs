using Core.Domains;

namespace Core.EndpointDefinitions;

public interface IEndpointDefinitionResolver
{
    EndpointDefinition? Resolve(string url);

    ValueTask<EndpointDefinition?> ResolveAsync(string url, CancellationToken cancellationToken = default);
}