using Core.Containers.Resolvers;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Core.Middlewares;

internal sealed class RequestExtractorMiddleware : IMiddleware
{
    private const string DisableKey = "disable";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var path = context.Request.Path.Value?.ToLower();
        var method = context.Request.Method.ToLower();
        if (InvalidRequestInfo())
        {
            await context.Response.SendNotFoundAsync();
            return;
        }

        var endpointDefinitionResolver = context.Resolve<IEndpointDefinitionResolver>();
        var (definition, pathParameters) = await endpointDefinitionResolver.ResolveAsync(path!, method);

        if (DefinitionNotFoundOrDisabled())
        {
            await context.Response.SendNotFoundAsync();
            return;
        }

        var state = context.ProcessorState<RequestState>();
        state.EndpointDefinition = new RequestEndpointDefinition
        {
            Method = definition!.Method,
            Meta = definition.Meta,
            Endpoint = definition.Endpoint,
            Pattern = definition.Pattern,
            BaseUrl = definition.BaseUrl,
            MapTo = definition.MapTo
        };

        dynamic? body = null;
        string? contentType = null;
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
                contentType = context.Request.ContentType!.StartsWith("multipart/form-data") ? RequestInfo.MultiPartFormData : RequestInfo.UrlEncodedFormDataContentType;
            }
        }

        state.RequestInfo = new RequestInfo
        {
            Headers = context.Request.Headers.ToDictionary(a => a.Key, a => string.Join(";", a.Value!)),
            Method = method,
            Body = body,
            Path = string.Format(definition.MapTo, pathParameters),
            QueryString = context.Request.QueryString.Value,
            ContentType = contentType
        };

        await next(context);
        return;

        bool InvalidRequestInfo() => string.IsNullOrEmpty(path) || string.IsNullOrEmpty(method);

        bool DefinitionNotFoundOrDisabled() => definition is null || definition.Meta.TryGetValue(DisableKey, out _);

        bool HasBody() => method is HttpRequestMethods.Post or HttpRequestMethods.Patch or HttpRequestMethods.Put && context.Request.ContentLength > 0;
    }
}