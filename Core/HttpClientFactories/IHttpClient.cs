namespace Core.HttpClientFactories;

public interface IHttpClient
{
    public ValueTask<object> SendAsync(RequestInfo req);

}