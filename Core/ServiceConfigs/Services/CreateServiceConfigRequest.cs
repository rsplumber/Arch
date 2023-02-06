namespace Core.ServiceConfigs.Services;

public sealed record CreateServiceConfigRequest
{
    public required string Name { get; init; } = default!;

    public Dictionary<string, string> Meta { get; set; } = new();
}