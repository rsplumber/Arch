using System.Text.Json;

namespace Application.Middlewares;

internal class RequestDispatcherMiddleware : IMiddleware
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string RequestInfoKey = "request_info";
    private const string ArchEndpointDefinitionKey = "arch_endpoint_definition";
    private const string IgnoreDispatchKey = "ignore_dispatch";
    private const string HttpFactoryName = "default";
    private const string BaseUrlMetaKey = "base_url";
    private const string ContentType = "application/json; charset=utf-8";


    public RequestDispatcherMiddleware(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        dynamic? info = context.Items[RequestInfoKey];
        dynamic? endpointDefinition = context.Items[ArchEndpointDefinitionKey];

        if (endpointDefinition is null || info is null) return;

        foreach (var meta in endpointDefinition.Meta)
        {
            if (meta.Key != IgnoreDispatchKey) continue;
            await next(context);
            return;
        }

        var client = _httpClientFactory.CreateClient(HttpFactoryName);
        client.DefaultRequestHeaders.Clear();
        if (info.Headers is not null)
        {
            foreach (var header in info.Headers)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        object? requestBody = null;
        if (info.Body is not null)
        {
            requestBody = JsonSerializer.Deserialize<object>(info.Body);
        }

        var httpResponse = info.Method switch
        {
            HttpRequestMethods.Get => await client.GetAsync(ApiUrl()),
            HttpRequestMethods.Delete => await client.DeleteAsync(ApiUrl()),
            HttpRequestMethods.Patch => await client.PatchAsJsonAsync(ApiUrl(), requestBody),
            HttpRequestMethods.Post => await client.PostAsJsonAsync(ApiUrl(), requestBody),
            HttpRequestMethods.Put => await client.PutAsJsonAsync(ApiUrl(), requestBody),
            _ => throw new ArgumentOutOfRangeException()
        };

        var response = await httpResponse.Content.ReadAsStringAsync();
        context.Response.ContentType = ContentType;
        await context.Response.WriteAsync(response);

        string ApiUrl()
        {
            string? baseUrl = null;
            foreach (var meta in endpointDefinition.Meta)
            {
                if (meta.Key != BaseUrlMetaKey) continue;
                baseUrl = meta.Value as string;
                break;
            }

            return $"{baseUrl}/{info.Path}";
        }
    }
}