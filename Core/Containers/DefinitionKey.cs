namespace Core.Containers;

public sealed record DefinitionKey
{
    private const string Separator = "$__$";


    private DefinitionKey(string pattern, string method)
    {
        Pattern = pattern;
        Method = method;
        Value = $"{Pattern}{Separator}{method}";
    }

    public static DefinitionKey From(string pattern, string method) => new(pattern, method);

    public static DefinitionKey? Parse(string key)
    {
        var split = key.Split(Separator);
        return split.Length != 2 ? null : From(split[0], split[1]);
    }

    public string Value { get; }

    public string Pattern { get; }

    public string Method { get; }

    public bool Equals(DefinitionKey? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    private sealed class ValueEqualityComparer : IEqualityComparer<DefinitionKey>
    {
        public bool Equals(DefinitionKey? x, DefinitionKey? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Value == y.Value;
        }

        public int GetHashCode(DefinitionKey obj)
        {
            return obj.Value.GetHashCode();
        }
    }

    public static IEqualityComparer<DefinitionKey> ValueComparer { get; } = new ValueEqualityComparer();
}