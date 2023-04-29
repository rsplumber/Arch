﻿using Core.EndpointDefinitions.Containers.Resolvers;
using Core.Library;

namespace Application.Middlewares;

internal sealed class RequestExtractorMiddleware : ArchMiddleware
{
    private readonly IEndpointDefinitionResolver _endpointDefinitionResolver;

    public RequestExtractorMiddleware(IEndpointDefinitionResolver endpointDefinitionResolver)
    {
        _endpointDefinitionResolver = endpointDefinitionResolver;
    }

    public override async Task HandleAsync(HttpContext context, RequestDelegate next)
    {
        context.Request.EnableBuffering();
        var path = ExtractPath();
        var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
        context.Request.Body.Position = 0;
        var method = context.Request.Method.ToLower();
        var (definition, pathParameters) = await _endpointDefinitionResolver.ResolveAsync(path, method);

        context.Items[ArchEndpointDefinitionKey] = definition is not null
            ? new RequestEndpointDefinition
            {
                Method = definition.Method,
                Meta = definition.Meta,
                Endpoint = definition.Endpoint,
                Pattern = definition.Pattern,
                BaseUrl = definition.BaseUrl
            }
            : null;

        context.Items[RequestInfoKey] = definition is not null
            ? new RequestInfo
            {
                Headers = context.Request.Headers.ToDictionary(a => a.Key, a => string.Join(";", a.Value!)),
                Method = method,
                Body = string.IsNullOrEmpty(body) ? null : body,
                Path = string.Format(definition.MapTo, pathParameters),
                QueryString = context.Request.QueryString.Value,
                ContentType = context.Request.ContentType
            }
            : null;

        await next(context);

        string ExtractPath()
        {
            var requestPath = context.Request.Path.Value!;
            var sanitizedPath = requestPath.StartsWith("/") ? requestPath.Remove(0, 1) : requestPath;
            return sanitizedPath.ToLower();
        }
    }
}