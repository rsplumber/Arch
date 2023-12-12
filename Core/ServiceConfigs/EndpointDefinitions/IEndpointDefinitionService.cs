namespace Arch.Core.ServiceConfigs.EndpointDefinitions;

public interface IEndpointDefinitionService
{
    ValueTask AddAsync(AddEndpointDefinitionRequest request, CancellationToken cancellationToken = default);

    ValueTask RemoveAsync(Guid endpointId, CancellationToken cancellationToken = default);
}

public sealed record AddEndpointDefinitionRequest
{
    public required Guid ServiceConfigId { get; init; }

    public required string Endpoint { get; init; } = default!;

    public required string MapTo { get; init; } = default!;

    public required HttpMethod Method { get; init; } = default!;

    public Dictionary<string, string> Meta { get; set; } = new();
}

public sealed record UpdateEndpointDefinitionRequest
{
    public required Guid Id { get; init; }

    public Dictionary<string, string> Meta { get; set; } = default!;
}