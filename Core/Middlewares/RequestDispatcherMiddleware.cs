﻿using System.Diagnostics;
using Core.Extensions;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Core.Middlewares;

internal sealed class RequestDispatcherMiddleware : IMiddleware
{
    private const string HttpFactoryName = "arch";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var requestState = context.ProcessorState<RequestState>();
        if (requestState.IgnoreDispatch())
        {
            await next(context);
            return;
        }

        var httpClient = CreateHttpClient();
        var watch = Stopwatch.StartNew();
        var httpResponseMessage = await httpClient.SendAsync(context.Request.Method(), requestState.ExtractApiUrl(), context.Request);
        watch.Stop();
        var response = await httpResponseMessage.ReadBodyAsync();
        requestState.ResponseInfo = new ResponseInfo
        {
            Code = (int)httpResponseMessage.StatusCode,
            Value = response,
            ResponseTimeMilliseconds = watch.ElapsedMilliseconds,
            ContentType = httpResponseMessage.ContentType()
        };
        await next(context).ConfigureAwait(false);
        return;

        HttpClient CreateHttpClient()
        {
            var client = context.Resolve<IHttpClientFactory>().CreateClient(HttpFactoryName);
            foreach (var (key, value) in context.Request.Headers())
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
            }

            foreach (var (key, value) in requestState.RequestInfo.AttachedHeaders)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
            }

            return client;
        }
    }
}