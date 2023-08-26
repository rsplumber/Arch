using Core.EndpointDefinitions;

namespace Core;

public interface IServiceEndpointResolver
{
    ValueTask<string> ResolveAsync(EndpointDefinition endpointDefinition, string apiUrl, CancellationToken cancellationToken = default);
}

internal sealed class BasicServiceEndpointResolver : IServiceEndpointResolver
{
    public ValueTask<string> ResolveAsync(EndpointDefinition endpointDefinition, string apiUrl, CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult($"{endpointDefinition.ServiceConfig.BaseUrl}/{apiUrl}");
    }
}