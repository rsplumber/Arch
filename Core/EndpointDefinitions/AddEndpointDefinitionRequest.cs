namespace Core.EndpointDefinitions;

public class AddEndpointDefinitionRequest
{
    public Guid ServiceConfigId { get; set; }

    public string Endpoint { get; set; } = default!;

    public Dictionary<string, string> Meta { get; set; } = new();
}