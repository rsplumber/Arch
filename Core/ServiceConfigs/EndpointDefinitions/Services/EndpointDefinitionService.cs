using Arch.Core.ServiceConfigs.EndpointDefinitions.Exceptions;
using Arch.Core.ServiceConfigs.Exceptions;
using Arch.EndpointGraph.Abstractions;

namespace Arch.Core.ServiceConfigs.EndpointDefinitions.Services;

public sealed class EndpointDefinitionService : IEndpointDefinitionService
{
    private readonly IEndpointGraph _endpointPatternTree;
    private readonly IServiceConfigRepository _serviceConfigRepository;
    private readonly IEndpointDefinitionRepository _endpointDefinitionRepository;

    public EndpointDefinitionService(IEndpointGraph endpointPatternTree, IServiceConfigRepository serviceConfigRepository, IEndpointDefinitionRepository endpointDefinitionRepository)
    {
        _endpointPatternTree = endpointPatternTree;
        _serviceConfigRepository = serviceConfigRepository;
        _endpointDefinitionRepository = endpointDefinitionRepository;
    }

    public async ValueTask AddAsync(AddEndpointDefinitionRequest request, CancellationToken cancellationToken = default)
    {
        var serviceConfig = await _serviceConfigRepository.FindAsync(request.ServiceConfigId, cancellationToken);
        if (serviceConfig is null)
        {
            throw new ServiceConfigNotFoundException();
        }

        if (serviceConfig.Primary)
        {
            throw new PrimaryServiceModificationException();
        }

        var sanitizedEndpoint = SanitizedEndpoint();
        await _endpointPatternTree.AddAsync(sanitizedEndpoint, cancellationToken);
        var (endpointPattern, _) = await _endpointPatternTree.FindAsync(request.Endpoint, cancellationToken);
        if (endpointPattern is null) throw new EndpointDefinitionNotFoundException();

        var endpointDefinition = new EndpointDefinition
        {
            Pattern = endpointPattern,
            Endpoint = sanitizedEndpoint,
            Method = request.Method,
            MapTo = request.MapTo
        };

        foreach (var (key, value) in request.Meta)
        {
            endpointDefinition.AddMeta(key, value);
        }

        serviceConfig.Add(endpointDefinition);

        await _serviceConfigRepository.UpdateAsync(serviceConfig, cancellationToken);
        return;

        string SanitizedEndpoint()
        {
            var sanitizedFirstIndex = request.Endpoint.StartsWith('/') ? request.Endpoint.Remove(0, 1) : request.Endpoint;
            var sanitized = sanitizedFirstIndex.EndsWith('/') ? request.Endpoint.Remove(sanitizedFirstIndex.Length - 1, 1) : sanitizedFirstIndex;
            return sanitized.ToLower();
        }
    }

    public async ValueTask RemoveAsync(Guid endpointId, CancellationToken cancellationToken = default)
    {
        var serviceConfig = await _serviceConfigRepository.FindByEndpointAsync(endpointId, cancellationToken);
        if (serviceConfig is null)
        {
            throw new ServiceConfigNotFoundException();
        }

        if (serviceConfig.Primary)
        {
            throw new PrimaryServiceModificationException();
        }

        var endpointDefinition = await _endpointDefinitionRepository.FindAsync(endpointId, cancellationToken);
        if (endpointDefinition is null)
        {
            throw new EndpointDefinitionNotFoundException();
        }

        serviceConfig.Remove(endpointDefinition);
        await _serviceConfigRepository.UpdateAsync(serviceConfig, cancellationToken);
    }
}