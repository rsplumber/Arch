namespace Core.EndpointDefinitions.Containers;

public sealed record ContainerEndpointDefinition : IComparable<ContainerEndpointDefinition>
{
    private ContainerEndpointDefinition(string serviceName)
    {
        ServiceName = serviceName;
    }

    public ContainerEndpointDefinition(string serviceName, string pattern, string endpoint, string method, Dictionary<string, string> meta)
    {
        ServiceName = serviceName;
        Pattern = pattern;
        Endpoint = endpoint;
        Method = method;
        Meta = meta;
    }

    public string ServiceName { get; } = default!;

    public string Pattern { get; } = default!;

    public string Endpoint { get; } = default!;

    public string Method { get; } = default!;

    public Dictionary<string, string> Meta { get; } = new();

    public bool Equals(ContainerEndpointDefinition? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Pattern == other.Pattern && Method == other.Method;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Pattern, Method);
    }


    public int CompareTo(ContainerEndpointDefinition? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        var patternComparison = string.Compare(Pattern, other.Pattern, StringComparison.Ordinal);
        if (patternComparison != 0) return patternComparison;
        return string.Compare(Endpoint, other.Endpoint, StringComparison.Ordinal);
    }

    public override string ToString()
    {
        return $"Pattern: {Pattern}, Method: {Method}";
    }
}