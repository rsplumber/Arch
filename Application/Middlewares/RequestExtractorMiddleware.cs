using Core.EndpointDefinitions;

namespace Application.Middlewares;

internal class RequestExtractorMiddleware : IMiddleware
{
    private readonly IEndpointDefinitionResolver _endpointDefinitionResolver;
    private const string RequestInfoKey = "request_info";
    private const string ArchEndpointDefinitionKey = "arch_endpoint_definition";

    public RequestExtractorMiddleware(IEndpointDefinitionResolver endpointDefinitionResolver)
    {
        _endpointDefinitionResolver = endpointDefinitionResolver;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var path = ExtractPath();
        var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
        context.Items[RequestInfoKey] = new RequestInfo
        {
            Headers = context.Request.Headers.ToDictionary(a => a.Key, a => string.Join(";", a.Value!)),
            Method = ExtractMethod(context.Request.Method),
            Body = string.IsNullOrEmpty(body) ? null : body,
            Path = path
        };
        context.Items[ArchEndpointDefinitionKey] = _endpointDefinitionResolver.Resolve(path);

        await next(context);

        string ExtractPath()
        {
            var requestPath = context.Request.Path.Value!;
            return requestPath.StartsWith("/") ? requestPath.Remove(0, 1) : requestPath;
        }
    }

    private static HttpRequestMethod ExtractMethod(string method) => Enum.Parse<HttpRequestMethod>(method);
}