using Core.Entities.EndpointDefinitions.Containers.Resolvers;
using Core.Middlewares.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Core.Middlewares;

internal sealed class RequestExtractorMiddleware : ArchMiddleware
{
    private readonly IEndpointDefinitionResolver _endpointDefinitionResolver;

    public RequestExtractorMiddleware(IEndpointDefinitionResolver endpointDefinitionResolver)
    {
        _endpointDefinitionResolver = endpointDefinitionResolver;
    }

    public override async Task HandleAsync(HttpContext context, RequestDelegate next)
    {
        var path = ExtractPath();
        var method = ExtractMethod();
        var (definition, pathParameters) = await _endpointDefinitionResolver.ResolveAsync(path, method);

        if (IsDisabled())
        {
            throw new EndpointNotFoundException();
        }

        context.Items[ArchEndpointDefinitionKey] = definition is not null
            ? new RequestEndpointDefinition
            {
                Method = definition.Method,
                Meta = definition.Meta,
                Endpoint = definition.Endpoint,
                Pattern = definition.Pattern,
                BaseUrl = definition.BaseUrl,
                MapTo = definition.MapTo
            }
            : null;

        string? body = null;
        if (HasBody())
        {
            var streamReader = new StreamReader(context.Request.Body);
            body = await streamReader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        context.Items[RequestInfoKey] = definition is not null
            ? new RequestInfo
            {
                Headers = context.Request.Headers.ToDictionary(a => a.Key, a => string.Join(";", a.Value!)),
                Method = method,
                Body = body,
                Path = string.Format(definition.MapTo, pathParameters),
                QueryString = context.Request.QueryString.Value,
                ContentType = context.Request.ContentType
            }
            : null;

        await next(context);

        string ExtractPath()
        {
            return context.Request.Path.Value is not null ? Sanitize(context.Request.Path.Value).ToLower() : string.Empty;

            string Sanitize(string rp)
            {
                var removedFirst = rp.StartsWith("/") ? rp.Remove(0, 1) : rp;
                return removedFirst.EndsWith("/") ? removedFirst.Remove(removedFirst.Length - 1, 1) : removedFirst;
            }
        }

        string ExtractMethod() => context.Request.Method.ToLower();

        bool HasBody() => method is HttpRequestMethods.Post or HttpRequestMethods.Patch or HttpRequestMethods.Put && context.Request.ContentLength > 0;
    }
}