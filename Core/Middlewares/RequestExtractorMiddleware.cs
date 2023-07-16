using Core.Containers.Resolvers;
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
        var path = context.Request.Path.Value.ToLower();
        var method = context.Request.Method.ToLower();
        var contentType = string.Empty;
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

        dynamic? body = null;
        if (HasBody())
        {
            var streamReader = new StreamReader(context.Request.Body);
            if (context.Request.ContentType == RequestInfo.ApplicationJsonContentType)
            {
                contentType = RequestInfo.ApplicationJsonContentType;
                body = await streamReader.ReadToEndAsync();
            }

            if (context.Request.HasFormContentType)
            {
                body = context.Request.Form;
                contentType = context.Request.ContentType.StartsWith("multipart/form-data") ? RequestInfo.MultiPartFormData : RequestInfo.UrlEncodedFormDataContentType;
            }

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
                ContentType = contentType
            }
            : null;

        await next(context);

        bool HasBody() => method is HttpRequestMethods.Post or HttpRequestMethods.Patch or HttpRequestMethods.Put && context.Request.ContentLength > 0;
    }
}