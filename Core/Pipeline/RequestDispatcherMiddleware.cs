using System.Diagnostics;
using Core.Extensions;
using Core.Pipeline.Models;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Core.Pipeline;

internal sealed class RequestDispatcherMiddleware : IMiddleware
{
    private const string HttpFactoryName = "arch";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var state = context.ProcessorState<RequestState>();
        if (state.IgnoreDispatch())
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        var httpClient = context.Resolve<IHttpClientFactory>().CreateClient(HttpFactoryName);
        foreach (var (key, value) in state.RequestInfo.Headers)
        {
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
        }

        var serviceEndpointResolver = context.Resolve<IServiceEndpointResolver>();
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

        if ((int)httpResponseMessage.StatusCode >= 500)
        {
            state.SetServiceUnavailable(requestElapsedTime);
            await next(context).ConfigureAwait(false);
            return;
        }

        state.Set(new ResponseInfo
        {
            Code = (int)httpResponseMessage.StatusCode,
            Value = await httpResponseMessage.ReadBodyAsync().ConfigureAwait(false),
            ResponseTimeMilliseconds = requestElapsedTime,
            ContentType = httpResponseMessage.ContentType(),
            Headers = httpResponseMessage.Headers()
        });

        await next(context).ConfigureAwait(false);
    }
}