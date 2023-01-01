namespace Arch;

public class Client : IHttpClient
{
    public async ValueTask<TResponse> GetRequestAsync<TResponse>(string url)
    {
        var client = new HttpClient();
        var httpResponseMessage = await client.GetAsync(url);
        var response = await httpResponseMessage.Content.ReadFromJsonAsync<TResponse>();
        if (response is null)
        {
            throw new ApplicationException();
        }

        return response;
    }

    public async ValueTask<TResponse> PostRequestAsync<TRequest, TResponse>(string url, TRequest request)
    {
        var client = new HttpClient();
        var httpResponseMessage = await client.PostAsJsonAsync(url, request);
        var response = await httpResponseMessage.Content.ReadFromJsonAsync<TResponse>();
        if (response is null)
        {
            throw new ApplicationException();
        }

        return response;
    }

    public async ValueTask<TResponse> PutRequestAsync<TRequest, TResponse>(string url, TRequest request)
    {
        var client = new HttpClient();
        var httpResponseMessage = await client.PutAsJsonAsync(url, request);
        var response = await httpResponseMessage.Content.ReadFromJsonAsync<TResponse>();
        if (response is null)
        {
            throw new ApplicationException();
        }

        return response;
    }

    public async ValueTask<TResponse> PatchRequestAsync<TRequest, TResponse>(string url, TRequest request)
    {
        var client = new HttpClient();
        var httpResponseMessage = await client.PatchAsJsonAsync(url, request);
        var response = await httpResponseMessage.Content.ReadFromJsonAsync<TResponse>();
        if (response is null)
        {
            throw new ApplicationException();
        }

        return response;
    }
}