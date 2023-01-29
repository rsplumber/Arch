using Core.ServiceConfigs;

namespace Core.EndpointDefinitions.Containers;

public interface IContainerInitializer
{
    Task InitializeAsync(List<ServiceConfig> serviceConfigs, CancellationToken cancellationToken = default);
}