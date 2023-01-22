using System.Collections.Concurrent;

namespace Core;

public struct TreeNode : IEquatable<TreeNode>
{
    private readonly string _item;
    private readonly ConcurrentDictionary<string, TreeNode> _children = new();
    private bool _end;
    private const string StartPathParameterKey = "{";
    private const string EndPathParameterKey = "}";
    private const string UrlSplitter = "/";
    private const string PathParameterKey = "##";
    private const string RootNodeKey = "root";

    public static TreeNode CreateRoot() => new(RootNodeKey);

    private static TreeNode Create(string key) => new(key);

    private TreeNode(string item)
    {
        _item = item;
    }

    public readonly void Append(string url)
    {
        using var urlArray = url.Split(UrlSplitter).AsEnumerable().GetEnumerator();
        var node = this;
        while (urlArray.MoveNext())
        {
            var key = IsPathParameter(urlArray.Current) ? PathParameterKey : urlArray.Current;
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

    public readonly string Find(string url)
    {
        return string.Join("/", FindEnumerable(url));
    }

    public readonly async ValueTask<string> FindAsync(string url, CancellationToken cancellationToken = new())
    {
        var foundedValues = await FindEnumerableAsync(url, cancellationToken);
        return string.Join("/", foundedValues);
    }

    public async void RemoveAsync(string url, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private readonly async ValueTask<List<string>> FindEnumerableAsync(string url, CancellationToken cancellationToken)
    {
        var currentNode = this;
        return await Task.Run(() => currentNode.FindEnumerable(url).ToList(), cancellationToken);
    }

    private readonly IEnumerable<string> FindEnumerable(string url)
    {
        using var urlArray = url.Split(UrlSplitter).AsEnumerable().GetEnumerator();
        var node = this;
        while (urlArray.MoveNext())
        {
            if (!node._children.TryGetValue(urlArray.Current, out var value))
            {
                if (!node._children.TryGetValue(PathParameterKey, out var pathValue) && !node._end)
                {
                    throw new Exception("not found");
                }

                value = pathValue;
            }

            node = value;
            yield return node._item;
        }
    }

    private static bool IsPathParameter(string key) =>
        key.StartsWith(StartPathParameterKey) && key.EndsWith(EndPathParameterKey);

    public bool Equals(TreeNode other)
    {
        return _item == other._item;
    }

    public override bool Equals(object? obj)
    {
        return obj is TreeNode other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _item.GetHashCode();
    }

    public static bool operator ==(TreeNode left, TreeNode right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TreeNode left, TreeNode right)
    {
        return !(left == right);
    }
}