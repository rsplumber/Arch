namespace Core.EndpointDefinitions.Services;

public class AddEndpointDefinitionRequest
{
    public Guid ServiceConfigId { get; set; }

    public string Endpoint { get; set; } = default!;

    public string Method { get; set; } = default!;

    public Dictionary<string, string> Meta { get; set; } = new();
}