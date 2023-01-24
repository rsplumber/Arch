using Core;

namespace Application.Middlewares;

internal class RequestExtractorMiddleware : IMiddleware
{
    private const string RequestInfoKey = "request_info";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.Items[RequestInfoKey] = new RequestInfo
        {
            Headers = context.Request.Headers.ToDictionary(a => a.Key, a => string.Join(";", a.Value!)),
            Method = ExtractMethod(context.Request.Method),
            Body = await context.Request.ReadFromJsonAsync<object>(),
            Path = ExtractPath()
        };
        await next(context);

        string ExtractPath()
        {
            var requestPath = context.Request.Path.Value!;
            return requestPath.StartsWith("/") ? requestPath.Remove(0, 1) : requestPath;
        }
    }

    private static HttpRequestMethod ExtractMethod(string method) => Enum.Parse<HttpRequestMethod>(method);
}