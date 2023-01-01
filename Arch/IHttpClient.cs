namespace Arch;

public interface IHttpClient
{
    public ValueTask<TResponse> GetRequestAsync<TResponse>(string url);

    public ValueTask<TResponse> PostRequestAsync<TRequest, TResponse>(string url, TRequest request);

    public ValueTask<TResponse> PutRequestAsync<TRequest, TResponse>(string url, TRequest request);

    public ValueTask<TResponse> PatchRequestAsync<TRequest, TResponse>(string url, TRequest request);
}