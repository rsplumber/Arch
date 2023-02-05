using Core.EndpointDefinitions.Exceptions;

namespace Core.EndpointDefinitions;

public sealed class EndpointNode
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

    public async ValueTask AppendAsync(string url, CancellationToken cancellationToken = new())
    {
        var currentNode = this;
        await Task.Run(() => currentNode.Append(url), cancellationToken);
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

    public string Find(string url)
    {
        return string.Join("/", FindEnumerable(url));
    }

    public async ValueTask<string> FindAsync(string url, CancellationToken cancellationToken = new())
    {
        var foundedValues = await FindEnumerableAsync(url, cancellationToken);
        return string.Join("/", foundedValues);
    }

    private async ValueTask<List<string>> FindEnumerableAsync(string url, CancellationToken cancellationToken)
    {
        var currentNode = this;
        return await Task.Run(() => currentNode.FindEnumerable(url).ToList(), cancellationToken);
    }

    private IEnumerable<string> FindEnumerable(string url)
    {
        using var urlArray = url.Split(UrlSplitter).AsEnumerable().GetEnumerator();
        var node = this;
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
                    throw new NodePathNotFoundException();
                }

                value = pathValue;
            }

            node = value ?? throw new NodePathNotFoundException();
            yield return node._item;
        }
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

    public static bool operator ==(EndpointNode left, EndpointNode right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(EndpointNode left, EndpointNode right)
    {
        return !(left == right);
    }
}