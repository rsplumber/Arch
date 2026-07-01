using Arch.Core.Pipeline.Models;

namespace Arch.Core;

public interface IServiceEndpointResolver
{
    ValueTask<string> ResolveAsync(ResolvedEndpoint endpoint, string apiUrl, CancellationToken cancellationToken = default);
}