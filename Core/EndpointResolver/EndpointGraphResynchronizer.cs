using Arch.Core.ServiceConfigs;

namespace Arch.Core.EndpointResolver;

public interface IEndpointGraphResynchronizer
{
    ValueTask ResyncAsync(CancellationToken cancellationToken = default);
}

internal sealed class EndpointGraphResynchronizer : IEndpointGraphResynchronizer
{
    private readonly IEndpointGraph _endpointGraph;
    private readonly IServiceConfigRepository _serviceConfigRepository;

    public EndpointGraphResynchronizer(IEndpointGraph endpointGraph, IServiceConfigRepository serviceConfigRepository)
    {
        _endpointGraph = endpointGraph;
        _serviceConfigRepository = serviceConfigRepository;
    }

    public async ValueTask ResyncAsync(CancellationToken cancellationToken = default)
    {
        var serviceConfigs = await _serviceConfigRepository.FindAsync(cancellationToken);
        var endpoints = serviceConfigs
            .SelectMany(serviceConfig => serviceConfig.EndpointDefinitions)
            .Select(definition => definition.Endpoint)
            .ToList();

        await _endpointGraph.RebuildAsync(endpoints, cancellationToken);
    }
}
