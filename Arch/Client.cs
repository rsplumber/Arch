namespace Arch;

public class Client : IHttpClient
{
    private readonly IConfiguration _configuration;

    public Client(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async ValueTask<TResponse> GetRequestAsync<TResponse>(string serviceName, string apiUrl)
    {
        var client = new HttpClient();
        var url = _configuration.GetSection("Services").GetSection(serviceName).GetValue<string>("BaseUrl") + apiUrl;
        var httpResponse = await client.GetAsync(url);
        var response = await httpResponse.Content.ReadFromJsonAsync<TResponse>();
        if (response is null)
        {
            throw new ApplicationException();
        }

        return response;
    }

    public async ValueTask<TResponse> PostRequestAsync<TRequest, TResponse>(string serviceName, string apiUrl, TRequest request)
    {
        var client = new HttpClient();
        var url = _configuration.GetSection("Services").GetSection(serviceName).GetValue<string>("BaseUrl") + apiUrl;
        var httpResponse = await client.PostAsJsonAsync(url, request);
        var response = await httpResponse.Content.ReadFromJsonAsync<TResponse>();
        if (response is null)
        {
            throw new ApplicationException();
        }

        return response;
    }

    public async ValueTask<TResponse> PutRequestAsync<TRequest, TResponse>(string serviceName, string url, TRequest request)
    {
        var client = new HttpClient();
        var httpResponse = await client.PutAsJsonAsync(url, request);
        var response = await httpResponse.Content.ReadFromJsonAsync<TResponse>();
        if (response is null)
        {
            throw new ApplicationException();
        }

        return response;
    }

    public async ValueTask<TResponse> PatchRequestAsync<TRequest, TResponse>(string serviceName, string url, TRequest request)
    {
        var client = new HttpClient();
        var httpResponse = await client.PatchAsJsonAsync(url, request);
        var response = await httpResponse.Content.ReadFromJsonAsync<TResponse>();
        if (response is null)
        {
            throw new ApplicationException();
        }

        return response;
    }
}