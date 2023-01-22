using Core.Domains;
using Core.Exceptions;
using Core.PatternTree;

namespace Core.EndpointDefinitions;

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

        _endpointPatternTree.Add(request.Endpoint);
        var endpointPattern = _endpointPatternTree.Find(request.Endpoint);

        var meta = request.Meta.Select(pair => new Meta
        {
            Id = pair.Key,
            Value = pair.Value
        }).ToList();
        meta.Add(new Meta
        {
            Id = "base_url",
            Value = serviceConfig.BaseUrl
        });
        var endpointDefinition = new EndpointDefinition
        {
            Pattern = endpointPattern,
            Endpoint = request.Endpoint,
            Meta = meta
        };

        serviceConfig.EndpointDefinitions.Add(endpointDefinition);

        await _serviceConfigRepository.UpdateAsync(serviceConfig, cancellationToken);
        await _endpointDefinitionContainer.AddAsync(endpointDefinition, cancellationToken);
    }

    public async ValueTask UpdateAsync(UpdateEndpointDefinitionRequest request, CancellationToken cancellationToken = default)
    {
        var serviceConfig = await _serviceConfigRepository.FindByEndpointAsync(request.UrlPattern, cancellationToken);
        if (serviceConfig is null)
        {
            throw new ServiceConfigNotFoundException();
        }

        await RemoveAsync(request.UrlPattern, cancellationToken);
        await AddAsync(new AddEndpointDefinitionRequest
        {
            ServiceConfigId = serviceConfig.Id,
            Endpoint = request.UrlPattern,
            Meta = request.Meta
        }, cancellationToken);
    }

    public async ValueTask RemoveAsync(string urlPattern, CancellationToken cancellationToken = default)
    {
        var serviceConfig = await _serviceConfigRepository.FindByEndpointAsync(urlPattern, cancellationToken);
        if (serviceConfig is null)
        {
            throw new ServiceConfigNotFoundException();
        }

        var endpointDefinition = await _endpointDefinitionRepository.FindAsync(urlPattern, cancellationToken);
        if (endpointDefinition is null)
        {
            throw new EndpointDefinitionNotFoundException();
        }

        serviceConfig.EndpointDefinitions.Remove(endpointDefinition);
        await _endpointPatternTree.RemoveAsync(urlPattern, cancellationToken);
        await _endpointDefinitionContainer.RemoveAsync(urlPattern, cancellationToken);
        await _serviceConfigRepository.UpdateAsync(serviceConfig, cancellationToken);
    }
}