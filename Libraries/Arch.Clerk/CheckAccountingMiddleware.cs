using Microsoft.AspNetCore.Http;

namespace Arch.Clerk;

internal class CheckAccountingMiddleware : IMiddleware
{
    private const string HttpClientFactoryKey = "clerk";
    private const string AccountingMetaKey = "accounting";
    private const string PayApi = "api/v1/pay";


    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        return next(context);
    }
}