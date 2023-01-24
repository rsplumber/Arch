using Core;
using Core.EndpointDefinitions;
using Core.RequestDispatcher;

namespace Application.Dispatcher;

public class RequestDispatcher : IRequestDispatcher
{
    private readonly IEndpointDefinitionResolver _endpointDefinitionResolver;
    private readonly IHttpClientFactory _httpClientFactory;
    private const string HttpFactoryName = "default";
    private const string BaseUrlMetaKey = "base_url";

    public RequestDispatcher(IEndpointDefinitionResolver endpointDefinitionResolver, IHttpClientFactory httpClientFactory)
    {
        _endpointDefinitionResolver = endpointDefinitionResolver;
        _httpClientFactory = httpClientFactory;
    }

    public async ValueTask<string?> ExecuteAsync(RequestInfo req)
    {
        var endpointDefinition = _endpointDefinitionResolver.Resolve(req.Path);
        if (endpointDefinition is null) return default;
        var client = _httpClientFactory.CreateClient(HttpFactoryName);
        client.DefaultRequestHeaders.Clear();
        if (req.Headers is not null)
        {
            foreach (var (key, value) in req.Headers)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
            }
        }

        var httpResponse = req.Method switch
        {
            HttpRequestMethod.GET => await client.GetAsync(ApiUrl()),
            HttpRequestMethod.DELETE => await client.DeleteAsync(ApiUrl()),
            HttpRequestMethod.PATCH => await client.PatchAsJsonAsync(ApiUrl(), req.Body),
            HttpRequestMethod.POST => await client.PostAsJsonAsync(ApiUrl(), req.Body),
            HttpRequestMethod.PUT => await client.PutAsJsonAsync(ApiUrl(), req.Body),
            HttpRequestMethod.UNKNOWN => throw new ArgumentOutOfRangeException(),
            _ => throw new ArgumentOutOfRangeException()
        };
        return await httpResponse.Content.ReadAsStringAsync();

        string ApiUrl()
        {
            var baseUrl = endpointDefinition.Meta.Find(meta => meta.Id == BaseUrlMetaKey)!.Value;
            return $"{baseUrl}/{endpointDefinition.Endpoint}";
        }
    }
}