using Core.EndpointDefinitions.Resolvers;

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
        context.Request.EnableBuffering();
        var path = ExtractPath();
        var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
        context.Request.Body.Position = 0;
        var method = context.Request.Method.ToLower();
        context.Items[RequestInfoKey] = new RequestInfo
        {
            Headers = context.Request.Headers.ToDictionary(a => a.Key, a => string.Join(";", a.Value!)),
            Method = method,
            Body = string.IsNullOrEmpty(body) ? null : body,
            Path = path
        };
        context.Items[ArchEndpointDefinitionKey] = _endpointDefinitionResolver.Resolve(path, method);

        await next(context);

        string ExtractPath()
        {
            var requestPath = context.Request.Path.Value!;
            return requestPath.StartsWith("/") ? requestPath.Remove(0, 1) : requestPath;
        }
    }
}