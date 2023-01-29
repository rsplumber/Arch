namespace Core.EndpointDefinitions.Services;

public class UpdateEndpointDefinitionRequest
{
    public Guid Id { get; set; } = default!;

    public Dictionary<string, string> Meta { get; set; } = default!;
}