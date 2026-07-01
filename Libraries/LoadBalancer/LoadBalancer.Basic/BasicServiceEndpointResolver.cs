using Arch.Core;
using Arch.Core.Pipeline.Models;

namespace Arch.LoadBalancer.Basic;

internal sealed class BasicServiceEndpointResolver : IServiceEndpointResolver
{
    public ValueTask<string> ResolveAsync(ResolvedEndpoint endpoint, string apiUrl, CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult($"{endpoint.Service.BaseUrls[0]}/{apiUrl}");
    }
}