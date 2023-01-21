namespace Core.HttpRequestHandler;

public interface IHttpRequestHandler
{
    public ValueTask<object> HandleAsync(string url,
        HttpRequestMethod method,
        object? body = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);
}