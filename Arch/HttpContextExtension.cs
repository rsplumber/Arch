using Core;

namespace Arch;

public static class HttpContextExtension
{
    public static RequestInfo RequestDispatcher(this HttpContext httpContext)
    {
        return new RequestInfo
        {
            Body = httpContext.Request.Body,
            Headers = httpContext.Request.Headers.ToDictionary(a => a.Key, a => string.Join(";", a.Value)),
            Method = httpContext.Request.Method,
            Path = httpContext.Request.Path
        };
    }
}