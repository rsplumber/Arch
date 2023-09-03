using Arch.Core;
using Arch.Core.EndpointDefinitions;

namespace Arch.LoadBalancer.Basic;

internal sealed class BasicServiceEndpointResolver : IServiceEndpointResolver
{
    public ValueTask<string> ResolveAsync(EndpointDefinition endpointDefinition, string apiUrl, CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult($"{endpointDefinition.ServiceConfig.BaseUrl}/{apiUrl}");
    }
}