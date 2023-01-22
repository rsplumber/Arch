namespace Core.EndpointDefinitions;

public interface IEndpointDefinitionService
{
    ValueTask AddAsync(AddEndpointDefinitionRequest request, CancellationToken cancellationToken = default);
    
    ValueTask UpdateAsync(UpdateEndpointDefinitionRequest request, CancellationToken cancellationToken = default);

    ValueTask RemoveAsync(string urlPattern, CancellationToken cancellationToken = default);
}