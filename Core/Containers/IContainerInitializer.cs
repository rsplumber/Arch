using Core.Entities.ServiceConfigs;

namespace Core.Containers;

public interface IContainerInitializer
{
    Task InitializeAsync(List<ServiceConfig> serviceConfigs, CancellationToken cancellationToken = default);
}