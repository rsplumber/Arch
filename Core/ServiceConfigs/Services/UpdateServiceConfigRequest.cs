namespace Core.ServiceConfigs.Services;

public sealed record UpdateServiceConfigRequest
{
    public required Guid Id { get; init; }

    public required string Name { get; init; } = default!;

    public Dictionary<string, string> Meta { get; set; } = new();
}