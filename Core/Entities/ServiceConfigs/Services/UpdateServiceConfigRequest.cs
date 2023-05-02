namespace Core.Entities.ServiceConfigs.Services;

public sealed record UpdateServiceConfigRequest
{
    public required Guid Id { get; init; }

    public required string Name { get; init; } = default!;

    public required string BaseUrl { get; init; } = default!;

    public Dictionary<string, string> Meta { get; set; } = new();
}