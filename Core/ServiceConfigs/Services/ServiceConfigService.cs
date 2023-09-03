using Arch.Core.ServiceConfigs.Events;
using Arch.Core.ServiceConfigs.Exceptions;
using DotNetCore.CAP;

namespace Arch.Core.ServiceConfigs.Services;

public sealed class ServiceConfigService : IServiceConfigService
{
    private readonly IServiceConfigRepository _serviceConfigRepository;
    private readonly ICapPublisher _capPublisher;

    public ServiceConfigService(IServiceConfigRepository serviceConfigRepository, ICapPublisher capPublisher)
    {
        _serviceConfigRepository = serviceConfigRepository;
        _capPublisher = capPublisher;
    }

    public async Task CreateAsync(CreateServiceConfigRequest request, CancellationToken cancellationToken = default)
    {
        var serviceConfig = ServiceConfig.Create(request.Name, request.BaseUrl, request.Meta);
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

        serviceConfig.Update(request.Name, request.BaseUrl, request.Meta);
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
        var serviceConfigRemovedEvent = new ServiceConfigRemovedEvent(serviceConfig.Id);
        await _capPublisher.PublishAsync(serviceConfigRemovedEvent.Name, serviceConfigRemovedEvent, cancellationToken: cancellationToken);
    }
}