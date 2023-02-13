namespace Core.EndpointDefinitions.Containers;

public sealed record ContainerEndpointDefinition : IComparable<ContainerEndpointDefinition>
{
    public required string ServiceName { get; init; } = default!;

    public required string BaseUrl { get; init; } = default!;

    public required string Pattern { get; init; } = default!;

    public required string Endpoint { get; init; } = default!;

    public required string Method { get; init; } = default!;

    public required Dictionary<string, string> Meta { get; init; } = new();

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