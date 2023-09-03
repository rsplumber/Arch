namespace Arch.Core.ServiceConfigs.Services;

public interface IServiceConfigService
{
    Task CreateAsync(CreateServiceConfigRequest request, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateServiceConfigRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}