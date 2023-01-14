using System.Text.Json;
using Core;

namespace Arch;

public static class HttpRequestExtension
{
    public static RequestInfo RequestInfoExecutor(this HttpRequest request) => new()
        {
            Body = request.Body,
            Headers = request.Headers.ToDictionary(a => a.Key, a => string.Join(";", a.Value)),
            Method = request.Method,
            Path = request.Path
        };
}