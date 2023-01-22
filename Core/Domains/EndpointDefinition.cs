namespace Core.Domains;

public class EndpointDefinition
{
    public string Pattern { get; set; } = default!;

    public string Endpoint { get; set; } = default!;

    public List<Meta> Meta { get; set; } = new();
}