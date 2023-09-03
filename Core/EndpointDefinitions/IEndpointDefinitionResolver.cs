namespace Arch.Core.EndpointDefinitions;

public interface IEndpointDefinitionResolver
{
    ValueTask<(EndpointDefinition?, object[])> ResolveAsync(string url, HttpMethod method, CancellationToken cancellationToken = default);
}