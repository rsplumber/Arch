using Core.Metas;

namespace Core.EndpointDefinitions;

public class EndpointDefinition
{
    public Guid Id { get; set; }

    public string Pattern { get; set; } = default!;

    public string Endpoint { get; set; } = default!;

    public string Method { get; set; } = default!;

    public List<Meta> Meta { get; set; } = new();
}