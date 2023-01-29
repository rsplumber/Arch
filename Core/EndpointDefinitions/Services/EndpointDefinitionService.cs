using Core.EndpointDefinitions.Containers;
using Core.EndpointDefinitions.Exceptions;
using Core.Metas;
using Core.ServiceConfigs;
using Core.ServiceConfigs.Exceptions;

namespace Core.EndpointDefinitions.Services;

public class EndpointDefinitionService : IEndpointDefinitionService
{
    private readonly IEndpointPatternTree _endpointPatternTree;
    private readonly IEndpointDefinitionContainer _endpointDefinitionContainer;
    private readonly IServiceConfigRepository _serviceConfigRepository;
    private readonly IEndpointDefinitionRepository _endpointDefinitionRepository;

    public EndpointDefinitionService(IEndpointPatternTree endpointPatternTree, IEndpointDefinitionContainer endpointDefinitionContainer, IServiceConfigRepository serviceConfigRepository, IEndpointDefinitionRepository endpointDefinitionRepository)
    {
        _endpointPatternTree = endpointPatternTree;
        _endpointDefinitionContainer = endpointDefinitionContainer;
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

        _endpointPatternTree.Add(SanitizedEndpoint());
        var endpointPattern = _endpointPatternTree.Find(request.Endpoint);


        var endpointDefinition = new EndpointDefinition
        {
            Pattern = endpointPattern,
            Endpoint = request.Endpoint,
            Method = request.Method.ToLower()
        };

        AddServiceConfigMetaToEndpoint();

        foreach (var (key, value) in request.Meta)
        {
            endpointDefinition.Meta.Add(new Meta
            {
                Id = key,
                Value = value
            });
        }

        serviceConfig.EndpointDefinitions.Add(endpointDefinition);

        await _serviceConfigRepository.UpdateAsync(serviceConfig, cancellationToken);

        await _endpointDefinitionContainer.AddAsync(endpointDefinition, cancellationToken);

        string SanitizedEndpoint() => request.Endpoint.StartsWith("/") ? request.Endpoint.Remove(0, 1) : request.Endpoint;

        void AddServiceConfigMetaToEndpoint() => endpointDefinition.Meta.AddRange(serviceConfig.Meta);
    }

    public async ValueTask UpdateAsync(UpdateEndpointDefinitionRequest request, CancellationToken cancellationToken = default)
    {
        var serviceConfig = await _serviceConfigRepository.FindByEndpointAsync(request.Id, cancellationToken);
        if (serviceConfig is null)
        {
            throw new ServiceConfigNotFoundException();
        }

        var endpointDefinition = await _endpointDefinitionRepository.FindAsync(request.Id, cancellationToken);
        if (endpointDefinition is null)
        {
            throw new EndpointDefinitionNotFoundException();
        }

        await RemoveAsync(endpointDefinition.Id, cancellationToken);
        await AddAsync(new AddEndpointDefinitionRequest
        {
            ServiceConfigId = serviceConfig.Id,
            Endpoint = endpointDefinition.Endpoint,
            Method = endpointDefinition.Method,
            Meta = request.Meta
        }, cancellationToken);
    }

    public async ValueTask RemoveAsync(Guid endpointId, CancellationToken cancellationToken = default)
    {
        var serviceConfig = await _serviceConfigRepository.FindByEndpointAsync(endpointId, cancellationToken);
        if (serviceConfig is null)
        {
            throw new ServiceConfigNotFoundException();
        }

        var endpointDefinition = await _endpointDefinitionRepository.FindAsync(endpointId, cancellationToken);
        if (endpointDefinition is null)
        {
            throw new EndpointDefinitionNotFoundException();
        }

        serviceConfig.EndpointDefinitions.Remove(endpointDefinition);
        await _endpointPatternTree.RemoveAsync(endpointDefinition.Pattern, cancellationToken);
        await _endpointDefinitionContainer.RemoveAsync(DefinitionKey.From(endpointDefinition.Pattern, endpointDefinition.Method), cancellationToken);
        await _serviceConfigRepository.UpdateAsync(serviceConfig, cancellationToken);
    }
}