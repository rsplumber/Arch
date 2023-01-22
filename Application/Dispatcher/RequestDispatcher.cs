using Core;
using Core.EndpointDefinitions;
using Core.RequestDispatcher;

namespace Application.Dispatcher;

public class RequestDispatcher : IRequestDispatcher
{
    private readonly IEndpointDefinitionResolver _endpointDefinitionResolver;
    private readonly IHttpClientFactory _httpClientFactory;
    private const string HttpFactoryName = "default";

    public RequestDispatcher(IEndpointDefinitionResolver endpointDefinitionResolver, IHttpClientFactory httpClientFactory)
    {
        _endpointDefinitionResolver = endpointDefinitionResolver;
        _httpClientFactory = httpClientFactory;
    }

    public async ValueTask<object?> ExecuteAsync(RequestInfo req)
    {
        var endpointDefinition = _endpointDefinitionResolver.Resolve(req.Path);
        if (endpointDefinition is null) return default;
        var client = _httpClientFactory.CreateClient(HttpFactoryName);
        if (req.Headers is not null)
        {
            foreach (var (key, value) in req.Headers)
            {
                client.DefaultRequestHeaders.Add(key, value);
            }
        }

        var httpResponse = req.Method switch
        {
            HttpRequestMethod.GET => await client.GetAsync(endpointDefinition.Endpoint),
            HttpRequestMethod.DELETE => await client.DeleteAsync(endpointDefinition.Endpoint),
            HttpRequestMethod.PATCH => await client.PatchAsJsonAsync(endpointDefinition.Endpoint, req.Body),
            HttpRequestMethod.POST => await client.PostAsJsonAsync(endpointDefinition.Endpoint, req.Body),
            HttpRequestMethod.PUT => await client.PutAsJsonAsync(endpointDefinition.Endpoint, req.Body),
            HttpRequestMethod.UNKNOWN => throw new ArgumentOutOfRangeException(),
            _ => throw new ArgumentOutOfRangeException()
        };
        return await httpResponse.Content.ReadFromJsonAsync<object?>();
    }
}