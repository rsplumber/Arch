using Core;
using Core.RequestDispatcher;

namespace Application.Middlewares;

internal class RequestDispatcherMiddleware : IMiddleware
{
    private const string RequestInfoKey = "request_info";
    private readonly IRequestDispatcher _requestDispatcher;

    public RequestDispatcherMiddleware(IRequestDispatcher requestDispatcher)
    {
        _requestDispatcher = requestDispatcher;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var info = context.Items[RequestInfoKey] as RequestInfo;
        var response = await _requestDispatcher.ExecuteAsync(info!) ?? string.Empty;
        await context.Response.WriteAsync(response);
    }
}