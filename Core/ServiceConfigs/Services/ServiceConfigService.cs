using Core.Metas;
using Core.ServiceConfigs.Exceptions;

namespace Core.ServiceConfigs.Services;

public class ServiceConfigService : IServiceConfigService
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
            Meta = CalculateMeta(request.Meta)
        };

        await _serviceConfigRepository.AddAsync(serviceConfig, cancellationToken);
    }

    public async Task UpdateAsync(UpdateServiceConfigRequest request, CancellationToken cancellationToken = default)
    {
        var serviceConfig = await _serviceConfigRepository.FindAsync(request.Id, cancellationToken);
        if (serviceConfig is null)
        {
            throw new ServiceConfigNotFoundException();
        }

        serviceConfig.Name = request.Name;
        serviceConfig.Meta = CalculateMeta(request.Meta);

        await _serviceConfigRepository.UpdateAsync(serviceConfig, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var serviceConfig = await _serviceConfigRepository.FindAsync(id, cancellationToken);
        if (serviceConfig is null)
        {
            throw new ServiceConfigNotFoundException();
        }

        await _serviceConfigRepository.DeleteAsync(serviceConfig, cancellationToken);
    }

    private static List<Meta> CalculateMeta(Dictionary<string, string> meta)
    {
        var finalMeta = new List<Meta>();
        AddAndSanitizeBaseUrlForMeta();
        foreach (var (key, value) in meta)
        {
            finalMeta.Add(new Meta
            {
                Id = key,
                Value = value
            });
        }

        return finalMeta;

        void AddAndSanitizeBaseUrlForMeta()
        {
            if (!meta.TryGetValue(BaseUrlKey, out var value)) throw new ApplicationException("base_url must define for service meta");
            finalMeta.Add(new Meta
            {
                Id = BaseUrlKey,
                Value = value
            });
            meta.Remove(BaseUrlKey);
        }
    }
}