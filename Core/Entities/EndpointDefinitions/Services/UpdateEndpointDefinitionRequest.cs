namespace Core.Entities.EndpointDefinitions.Services;

public sealed record UpdateEndpointDefinitionRequest
{
    public required Guid Id { get; init; }

    public Dictionary<string, string> Meta { get; set; } = default!;
}