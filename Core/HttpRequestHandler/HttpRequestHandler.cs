namespace Core.HttpRequestHandler;

internal class HttpRequestHandler : IHttpRequestHandler
{
    public async ValueTask<object> HandleAsync(string url,
        HttpRequestMethod method,
        object? body = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}