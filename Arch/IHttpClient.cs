namespace Arch;

public interface IHttpClient
{
    public ValueTask<TResponse> GetRequestAsync<TResponse>(string serviceName, string apiUrl);

    public ValueTask<TResponse> PostRequestAsync<TRequest, TResponse>(string serviceName, string apiUrl, TRequest request);

    public ValueTask<TResponse> PutRequestAsync<TRequest, TResponse>(string serviceName, string apiUrl, TRequest request);

    public ValueTask<TResponse> PatchRequestAsync<TRequest, TResponse>(string serviceName, string apiUrl, TRequest request);
}