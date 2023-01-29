namespace Core.ServiceConfigs.Services;

public class UpdateServiceConfigRequest
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public Dictionary<string, string> Meta { get; set; } = new();
}