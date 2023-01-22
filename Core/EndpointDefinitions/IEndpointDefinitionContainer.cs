using Core.Domains;

namespace Core.EndpointDefinitions;

public interface IEndpointDefinitionContainer
{
    ValueTask AddAsync(EndpointDefinition endpointDefinition, CancellationToken cancellationToken = default);

    ValueTask<EndpointDefinition?> GetAsync(string key, CancellationToken cancellationToken = default);

    EndpointDefinition? Get(string pattern);

    ValueTask RemoveAsync(string pattern, CancellationToken cancellationToken = default);
}