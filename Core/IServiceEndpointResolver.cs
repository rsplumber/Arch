using Arch.Core.EndpointDefinitions;

namespace Arch.Core;

public interface IServiceEndpointResolver
{
    ValueTask<string> ResolveAsync(EndpointDefinition endpointDefinition, string apiUrl, CancellationToken cancellationToken = default);
}