using System.Text.Json;
using Core.EndpointDefinitions;

namespace Application.Middlewares;

internal class RequestDispatcherMiddleware : IMiddleware
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string RequestInfoKey = "request_info";
    private const string ArchEndpointDefinitionKey = "arch_endpoint_definition";
    private const string HttpFactoryName = "default";
    private const string BaseUrlMetaKey = "base_url";
    private const string ContentType = "application/json; charset=utf-8";


    public RequestDispatcherMiddleware(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var info = context.Items[RequestInfoKey] as RequestInfo;
        var endpointDefinition = context.Items[ArchEndpointDefinitionKey] as EndpointDefinition;
        if (endpointDefinition is null || info is null) return;

        var client = _httpClientFactory.CreateClient(HttpFactoryName);
        client.DefaultRequestHeaders.Clear();
        if (info.Headers is not null)
        {
            foreach (var (key, value) in info.Headers)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
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
            var baseUrl = endpointDefinition.Meta.Find(meta => meta.Id == BaseUrlMetaKey)!.Value;
            return $"{baseUrl}/{info.Path}";
        }
    }
}