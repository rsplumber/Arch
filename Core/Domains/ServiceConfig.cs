namespace Core.Domains;

public class ServiceConfig
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string BaseUrl { get; set; } = default!;

    public List<EndpointDefinition> EndpointDefinitions { get; set; } = new();

    public List<Meta> Meta { get; set; } = new();
}