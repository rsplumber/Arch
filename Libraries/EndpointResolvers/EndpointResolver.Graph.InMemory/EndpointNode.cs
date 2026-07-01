using System.Buffers;
using System.Runtime.CompilerServices;

namespace Arch.EndpointResolver.Graph.InMemory;

internal sealed class EndpointNode
{
    private readonly string _item;
    private readonly Dictionary<string, EndpointNode> _children = new(StringComparer.Ordinal);
    private bool _end;

    private const char Separator = '/';
    private const char QueryParamChar = '?';
    private const char PathParamOpen = '{';
    private const char PathParamClose = '}';
    private const string PathParamKey = "##";
    private const string RootKey = "root";

    public static EndpointNode CreateRoot() => new(RootKey);

    private EndpointNode(string item) => _item = item;

    public void Append(string url)
    {
        var span = url.AsSpan();
        var node = this;

        foreach (var range in span.Split(Separator))
        {
            var segment = span[range];
            if (segment.IsEmpty) continue;

            if (IsQueryParam(segment))
            {
                node._end = true;
                return;
            }

            var isParam = IsPathParam(segment);
            var lookup = node._children.GetAlternateLookup<ReadOnlySpan<char>>();
            var lookupKey = isParam ? PathParamKey.AsSpan() : segment;

            if (!lookup.TryGetValue(lookupKey, out var child))
            {
                var keyStr = isParam ? PathParamKey : segment.ToString();
                child = new EndpointNode(keyStr);
                node._children.TryAdd(keyStr, child);
            }

            node = child;
        }

        node._end = true;
    }

    public (string?, object[]) Find(string url)
    {
        var span = url.AsSpan();
        var node = this;

        var patternBuffer = ArrayPool<string>.Shared.Rent(16);
        var paramsBuffer = ArrayPool<object>.Shared.Rent(8);
        var patternLen = 0;
        var paramsLen = 0;

        try
        {
            foreach (var range in span.Split(Separator))
            {
                var segment = span[range];
                if (segment.IsEmpty) continue;
                if (IsQueryParam(segment)) break;

                var lookup = node._children.GetAlternateLookup<ReadOnlySpan<char>>();

                if (lookup.TryGetValue(segment, out var child))
                {
                    node = child;
                }
                else if (node._children.TryGetValue(PathParamKey, out var paramChild))
                {
                    if (paramsLen == paramsBuffer.Length) GrowBuffer(ref paramsBuffer, paramsLen);
                    paramsBuffer[paramsLen++] = segment.ToString();
                    node = paramChild;
                }
                else
                {
                    return (null, []);
                }

                if (patternLen == patternBuffer.Length) GrowBuffer(ref patternBuffer, patternLen);
                patternBuffer[patternLen++] = node._item;
            }

            if (patternLen == 0) return (null, []);

            var pattern = string.Join(Separator, patternBuffer.AsSpan(0, patternLen));
            var @params = paramsLen == 0 ? [] : paramsBuffer.AsSpan(0, paramsLen).ToArray();

            return (pattern, @params);
        }
        finally
        {
            ArrayPool<string>.Shared.Return(patternBuffer, clearArray: true);
            ArrayPool<object>.Shared.Return(paramsBuffer, clearArray: true);
        }
    }

    public void Remove(string urlPattern)
    {
        var span = urlPattern.AsSpan();
        Span<Range> segments = stackalloc Range[64];
        var count = span.Split(segments, Separator, StringSplitOptions.RemoveEmptyEntries);
        RemoveCore(span, segments[..count], 0);
    }

    private bool RemoveCore(ReadOnlySpan<char> url, ReadOnlySpan<Range> segments, int depth)
    {
        if (depth == segments.Length)
        {
            _end = false;
            return _children.Count == 0;
        }

        var segment = url[segments[depth]];
        var key = IsPathParam(segment) ? PathParamKey : segment.ToString();

        if (!_children.TryGetValue(key, out var child))
            return false;

        if (child.RemoveCore(url, segments, depth + 1))
            _children.Remove(key);

        return !_end && _children.Count == 0;
    }

    private static void GrowBuffer<T>(ref T[] buffer, int currentLen)
    {
        var next = ArrayPool<T>.Shared.Rent(buffer.Length * 2);
        buffer.AsSpan(0, currentLen).CopyTo(next);
        ArrayPool<T>.Shared.Return(buffer, clearArray: true);
        buffer = next;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsPathParam(ReadOnlySpan<char> segment)
        => segment.Length >= 2 && segment[0] == PathParamOpen && segment[^1] == PathParamClose;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsQueryParam(ReadOnlySpan<char> segment)
        => segment.Length > 0 && segment[0] == QueryParamChar;
}
