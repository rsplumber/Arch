namespace Core.ServiceConfigs.Services;

public class CreateServiceConfigRequest
{
    public string Name { get; set; } = default!;

    public Dictionary<string, string> Meta { get; set; } = new();
}