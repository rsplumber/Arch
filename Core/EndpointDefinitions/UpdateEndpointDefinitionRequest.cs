namespace Core.EndpointDefinitions;

public class UpdateEndpointDefinitionRequest
{
    public string UrlPattern { get; set; } = default!;

    public Dictionary<string, string> Meta { get; set; } = default!;
}