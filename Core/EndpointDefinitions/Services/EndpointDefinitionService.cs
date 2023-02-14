using Core.EndpointDefinitions.Containers;
using Core.EndpointDefinitions.Exceptions;
using Core.Metas;
using Core.ServiceConfigs;
using Core.ServiceConfigs.Exceptions;

namespace Core.EndpointDefinitions.Services;

public sealed class EndpointDefinitionService : IEndpointDefinitionService
{
    private readonly IEndpointPatternTree _endpointPatternTree;
    private readonly IServiceConfigRepository _serviceConfigRepository;
    private readonly IEndpointDefinitionRepository _endpointDefinitionRepository;
    private readonly IContainerInitializer _containerInitializer;

    public EndpointDefinitionService(IEndpointPatternTree endpointPatternTree, IServiceConfigRepository serviceConfigRepository, IEndpointDefinitionRepository endpointDefinitionRepository, IContainerInitializer containerInitializer)
    {
        _endpointPatternTree = endpointPatternTree;
        _serviceConfigRepository = serviceConfigRepository;
        _endpointDefinitionRepository = endpointDefinitionRepository;
        _containerInitializer = containerInitializer;
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
        _endpointPatternTree.Add(sanitizedEndpoint);
        var endpointPattern = _endpointPatternTree.Find(request.Endpoint);

        var endpointDefinition = new EndpointDefinition
        {
            Pattern = endpointPattern,
            Endpoint = sanitizedEndpoint,
            Method = request.Method.ToLower()
        };

        AddServiceConfigMetaToEndpoint();

        foreach (var (key, value) in request.Meta)
        {
            endpointDefinition.Meta.Add(new Meta
            {
                Key = key,
                Value = value
            });
        }

        serviceConfig.EndpointDefinitions.Add(endpointDefinition);

        await _serviceConfigRepository.UpdateAsync(serviceConfig, cancellationToken);

        await ReInitializeContainersAsync(cancellationToken);

        string SanitizedEndpoint()
        {
            var sanitizedFirstIndex = request.Endpoint.StartsWith("/") ? request.Endpoint.Remove(0, 1) : request.Endpoint;
            var sanitized = sanitizedFirstIndex.EndsWith("/") ? request.Endpoint.Remove(sanitizedFirstIndex.Length - 1, 1) : sanitizedFirstIndex;
            return sanitized.ToLower();
        }

        void AddServiceConfigMetaToEndpoint() => endpointDefinition.Meta.AddRange(serviceConfig.Meta);
    }

    public async ValueTask UpdateAsync(UpdateEndpointDefinitionRequest request, CancellationToken cancellationToken = default)
    {
        var serviceConfig = await _serviceConfigRepository.FindByEndpointAsync(request.Id, cancellationToken);
        if (serviceConfig is null)
        {
            throw new ServiceConfigNotFoundException();
        }

        if (serviceConfig.Primary)
        {
            throw new PrimaryServiceModificationException();
        }

        var endpointDefinition = serviceConfig.EndpointDefinitions.Find(definition => definition.Id == request.Id);
        if (endpointDefinition is null)
        {
            throw new EndpointDefinitionNotFoundException();
        }

        endpointDefinition.Meta.Clear();
        foreach (var (key, value) in request.Meta)
        {
            endpointDefinition.Meta.Add(new Meta
            {
                Key = key,
                Value = value
            });
        }

        await _serviceConfigRepository.UpdateAsync(serviceConfig, cancellationToken);
        await ReInitializeContainersAsync(cancellationToken);
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

        serviceConfig.EndpointDefinitions.Remove(endpointDefinition);
        await _serviceConfigRepository.UpdateAsync(serviceConfig, cancellationToken);

        await ReInitializeContainersAsync(cancellationToken);
    }

    private async Task ReInitializeContainersAsync(CancellationToken cancellationToken = default)
    {
        var serviceConfigs = await _serviceConfigRepository.FindAsync(cancellationToken);
        await _containerInitializer.InitializeAsync(serviceConfigs, cancellationToken);
    }
}