namespace Core.Entities.EndpointDefinitions.Services;

public interface IEndpointDefinitionService
{
    ValueTask AddAsync(AddEndpointDefinitionRequest request, CancellationToken cancellationToken = default);

    ValueTask UpdateAsync(UpdateEndpointDefinitionRequest request, CancellationToken cancellationToken = default);

    ValueTask RemoveAsync(Guid endpointId, CancellationToken cancellationToken = default);
}