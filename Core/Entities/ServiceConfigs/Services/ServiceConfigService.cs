using Core.Entities.Metas;
using Core.Entities.ServiceConfigs.Exceptions;

namespace Core.Entities.ServiceConfigs.Services;

public sealed class ServiceConfigService : IServiceConfigService
{
    private readonly IServiceConfigRepository _serviceConfigRepository;
    private const string BaseUrlKey = "base_url";

    public ServiceConfigService(IServiceConfigRepository serviceConfigRepository)
    {
        _serviceConfigRepository = serviceConfigRepository;
    }

    public async Task CreateAsync(CreateServiceConfigRequest request, CancellationToken cancellationToken = default)
    {
        var serviceConfig = new ServiceConfig
        {
            Name = request.Name,
            BaseUrl = request.BaseUrl
        };

        serviceConfig.Meta = CalculateMeta(serviceConfig, request.Meta);
        await _serviceConfigRepository.AddAsync(serviceConfig, cancellationToken);
    }

    public async Task UpdateAsync(UpdateServiceConfigRequest request, CancellationToken cancellationToken = default)
    {
        var serviceConfig = await _serviceConfigRepository.FindAsync(request.Id, cancellationToken);
        if (serviceConfig is null)
        {
            throw new ServiceConfigNotFoundException();
        }

        if (serviceConfig.Primary)
        {
            throw new PrimaryServiceModificationException();
        }

        serviceConfig.Name = request.Name;
        serviceConfig.BaseUrl = request.BaseUrl;
        serviceConfig.Meta = CalculateMeta(serviceConfig, request.Meta);

        await _serviceConfigRepository.UpdateAsync(serviceConfig, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var serviceConfig = await _serviceConfigRepository.FindAsync(id, cancellationToken);
        if (serviceConfig is null)
        {
            throw new ServiceConfigNotFoundException();
        }

        if (serviceConfig.Primary)
        {
            throw new PrimaryServiceModificationException();
        }

        await _serviceConfigRepository.DeleteAsync(serviceConfig, cancellationToken);
    }

    private static List<Meta> CalculateMeta(ServiceConfig serviceConfig, Dictionary<string, string> meta)
    {
        var finalMeta = new List<Meta>();
        AddAndSanitizeBaseUrlForMeta();
        foreach (var (key, value) in meta)
        {
            finalMeta.Add(new Meta
            {
                Key = key,
                Value = value
            });
        }

        return finalMeta;

        void AddAndSanitizeBaseUrlForMeta()
        {
            finalMeta.Add(new Meta
            {
                Key = BaseUrlKey,
                Value = serviceConfig.BaseUrl
            });
            meta.Remove(BaseUrlKey);
        }
    }
}