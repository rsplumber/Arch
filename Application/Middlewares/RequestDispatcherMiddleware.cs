using Core.Domains;

namespace Application.Middlewares;

internal class RequestDispatcherMiddleware : IMiddleware
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string RequestInfoKey = "request_info";
    private const string ArchEndpointDefinitionKey = "arch_endpoint_definition";
    private const string HttpFactoryName = "default";
    private const string BaseUrlMetaKey = "base_url";


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

        var httpResponse = info.Method switch
        {
            HttpRequestMethod.GET => await client.GetAsync(ApiUrl()),
            HttpRequestMethod.DELETE => await client.DeleteAsync(ApiUrl()),
            HttpRequestMethod.PATCH => await client.PatchAsJsonAsync(ApiUrl(), info.Body),
            HttpRequestMethod.POST => await client.PostAsJsonAsync(ApiUrl(), info.Body),
            HttpRequestMethod.PUT => await client.PutAsJsonAsync(ApiUrl(), info.Body),
            HttpRequestMethod.UNKNOWN => throw new ArgumentOutOfRangeException(),
            _ => throw new ArgumentOutOfRangeException()
        };

        var response = await httpResponse.Content.ReadAsStringAsync();
        await context.Response.WriteAsync(response);

        string ApiUrl()
        {
            var baseUrl = endpointDefinition.Meta.Find(meta => meta.Id == BaseUrlMetaKey)!.Value;
            return $"{baseUrl}/{endpointDefinition.Endpoint}";
        }
    }
}