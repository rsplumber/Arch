using System.Globalization;

namespace Core.Pipeline.Models;

public class RequestInfo
{
    public const string ApplicationJsonContentType = "application/json";
    public const string UrlEncodedFormDataContentType = "application/x-www-form-urlencoded";
    public const string MultiPartFormData = "multipart/form-data";

    public RequestInfo(HttpMethod method, string mapTo, object[] pathParameters, string? queryString)
    {
        Method = method;
        Path = $"{string.Format(CultureInfo.CurrentCulture, mapTo, pathParameters)}{queryString}";
    }

    public Guid RequestId { get; } = Guid.NewGuid();

    public HttpMethod Method { get; private set; }

    public string Path { get; private set; }

    public Dictionary<string, string> Headers { get; init; } = new();

    public DateTime RequestDateUtc { get; } = DateTime.UtcNow;
    
    
}