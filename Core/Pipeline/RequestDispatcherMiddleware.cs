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

        var httpClient = CreateHttpClient();
        var watch = Stopwatch.StartNew();
        var httpResponseMessage = await httpClient.SendAsync(
                state.RequestInfo.Method,
                state.ResolveDispatchApiUrl(),
                context.Request)
            .ConfigureAwait(false);
        watch.Stop();
        if (httpResponseMessage is null)
        {
            state.SetServiceUnavailableResponse();
            await next(context).ConfigureAwait(false);
            return;
        }

        var response = await httpResponseMessage.ReadBodyAsync().ConfigureAwait(false);
        state.Set(new ResponseInfo
        {
            Code = (int)httpResponseMessage.StatusCode,
            Value = response,
            ResponseTimeMilliseconds = watch.ElapsedMilliseconds,
            ContentType = httpResponseMessage.ContentType()
        });
        await next(context).ConfigureAwait(false);
        return;

        HttpClient CreateHttpClient()
        {
            var client = context.Resolve<IHttpClientFactory>().CreateClient(HttpFactoryName);
            foreach (var (key, value) in state.RequestInfo.Headers)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
            }

            return client;
        }
    }
}