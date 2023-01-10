namespace Core;

public class RequestInfo
{
    public string Method { get; set; }

    public string Path { get; set; }

    public Dictionary<string, string> Headers { get; set; }

    public object Body { get; set; }
}