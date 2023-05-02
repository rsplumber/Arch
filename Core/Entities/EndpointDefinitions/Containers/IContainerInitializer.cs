using Core.Entities.ServiceConfigs;

namespace Core.Entities.EndpointDefinitions.Containers;

public interface IContainerInitializer
{
    Task InitializeAsync(List<ServiceConfig> serviceConfigs, CancellationToken cancellationToken = default);
}