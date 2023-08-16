namespace EndpointGraph.InMemory;

internal sealed class EndpointNode
{
    private readonly string _item;
    private readonly Dictionary<string, EndpointNode> _children = new();
    private bool _end;
    private const string StartPathParameterKey = "{";
    private const string EndPathParameterKey = "}";
    private const string QueryParamKey = "?";
    private const string UrlSplitter = "/";
    private const string PathParameterKey = "##";
    private const string RootNodeKey = "root";

    public static EndpointNode CreateRoot() => new(RootNodeKey);

    private static EndpointNode Create(string key) => new(key);

    private EndpointNode(string item)
    {
        _item = item;
    }

    public void Append(string url)
    {
        using var urlArray = url.Split(UrlSplitter).AsEnumerable().GetEnumerator();
        var node = this;
        while (urlArray.MoveNext())
        {
            var key = IsPathParameter(urlArray.Current) ? PathParameterKey : urlArray.Current;
            if (IsQueryParameter(key))
            {
                node._end = true;
                break;
            }

            if (node._children.TryGetValue(key, out var value))
            {
                node = value;
                continue;
            }

            var newNode = Create(key);
            node._children.TryAdd(key, newNode);
            node = newNode;
        }

        node._end = true;
    }

    public (string?, object[]) Find(string url)
    {
        var (pattern, urlParams) = ExtractUrlData(url);
        return pattern.Length == 0 ? (null, Array.Empty<object>()) : (string.Join(UrlSplitter, pattern), urlParams);
    }

    private (string[], object[]) ExtractUrlData(string url)
    {
        using var urlArray = url.Split(UrlSplitter)
            .Where(s => !string.IsNullOrEmpty(s))
            .AsEnumerable()
            .GetEnumerator();
        var node = this;
        var urlParams = new List<object>();
        var urlPattern = new List<string>();
        while (urlArray.MoveNext())
        {
            if (IsQueryParameter(urlArray.Current))
            {
                break;
            }

            if (!node._children.TryGetValue(urlArray.Current, out var value))
            {
                if (!node._children.TryGetValue(PathParameterKey, out var pathValue) && !node._end)
                {
                    return NotFound();
                }

                urlParams.Add(urlArray.Current);
                value = pathValue;
            }

            if (value is null) return NotFound();
            node = value;
            urlPattern.Add(node._item);
        }

        return (urlPattern.ToArray(), urlParams.ToArray());

        (string[], object[]) NotFound() => (Array.Empty<string>(), Array.Empty<object>());
    }

    private static bool IsPathParameter(string key) => key.StartsWith(StartPathParameterKey) && key.EndsWith(EndPathParameterKey);

    private static bool IsQueryParameter(string key) => key.StartsWith(QueryParamKey);

    private bool Equals(EndpointNode other)
    {
        return _item == other._item;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is EndpointNode other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _item.GetHashCode();
    }
}