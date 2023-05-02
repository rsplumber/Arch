namespace Core.Entities.EndpointDefinitions.Services;

public sealed record AddEndpointDefinitionRequest
{
    public required Guid ServiceConfigId { get; init; }

    public required string Endpoint { get; init; } = default!;
    
    public required string MapTo { get; init; } = default!;

    public required string Method { get; init; } = default!;

    public Dictionary<string, string> Meta { get; set; } = new();
}