using System.Diagnostics;
using Arch.Core.Extensions.Http;
using Arch.Core.Pipeline.Models;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Arch.Core.Pipeline;

internal sealed class RequestDispatcherMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var state = context.RequestState();
        if (state.IgnoreDispatch())
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        var httpClient = context.HttpClient();
        foreach (var (key, value) in state.RequestInfo.Headers)
        {
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
        }

        var serviceEndpointResolver = context.LoadBalancer();
        var apiPath = await serviceEndpointResolver.ResolveAsync(state.EndpointDefinition, state.RequestInfo.Path);

        var watch = Stopwatch.StartNew();
        var httpResponseMessage = await httpClient.SendAsync(
                state.RequestInfo.Method,
                apiPath,
                context.Request)
            .ConfigureAwait(false);
        watch.Stop();
        var requestElapsedTime = watch.ElapsedMilliseconds;

        if (httpResponseMessage is null)
        {
            state.SetServiceTimeOut(requestElapsedTime);
            await next(context).ConfigureAwait(false);
            return;
        }

        switch ((int)httpResponseMessage.StatusCode)
        {
            case >= 500:
                state.SetServiceUnavailable(requestElapsedTime);
                await next(context).ConfigureAwait(false);
                return;
            case 401:
            case 403:
                state.SetUnAuthorized((int)httpResponseMessage.StatusCode, requestElapsedTime);
                await next(context).ConfigureAwait(false);
                return;
            case 302:
                await context.Response.SendRedirectAsync(httpResponseMessage.Headers.Location!.ToString(), true).ConfigureAwait(false);
                return;
            default:
                state.Set(new ResponseInfo
                {
                    Code = (int)httpResponseMessage.StatusCode,
                    Value = await httpResponseMessage.ReadBodyAsync().ConfigureAwait(false),
                    ResponseTimeMilliseconds = requestElapsedTime,
                    ContentType = httpResponseMessage.ContentType(),
                    Headers = httpResponseMessage.Headers()
                });

                await next(context).ConfigureAwait(false);
                break;
        }
    }
}