using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Core.Middlewares;

internal sealed class RequestDispatcherMiddleware : IMiddleware
{
    private const string HttpFactoryName = "arch";
    private const string ApplicationJsonMediaType = "application/json";
    private const string ApplicationProblemJsonMediaType = "application/problem+json";
    private const string IgnoreDispatchKey = "ignore_dispatch";

    private static readonly JsonSerializerOptions DefaultSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var requestState = context.ProcessorState<RequestState>();
        if (IgnoreDispatch())
        {
            await next(context);
            return;
        }

        var client = context.Resolve<IHttpClientFactory>().CreateClient(HttpFactoryName);
        foreach (var header in requestState.RequestInfo.Headers)
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var header in requestState.RequestInfo.AttachedHeaders)
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }

        var httpMethod = MapToHttpMethod();
        if (httpMethod is null)
        {
            await context.Response.SendAsync("Invalid HttpMethod", 400);
            return;
        }

        var message = new HttpRequestMessage(httpMethod, ExtractApiUrl());
        if (requestState.RequestInfo.Body is not null)
        {
            switch (requestState.RequestInfo.ContentType)
            {
                case RequestInfo.ApplicationJsonContentType:
                    message.Content = JsonContent.Create(JsonSerializer.Deserialize<object>(requestState.RequestInfo.Body));
                    break;
                case RequestInfo.MultiPartFormData:
                    var multiPartFormCollection = (IFormCollection)requestState.RequestInfo.Body;
                    var multipartFormDataContent = new MultipartFormDataContent();
                    foreach (var keyValuePair in multiPartFormCollection)
                    {
                        multipartFormDataContent.Add(new StringContent(keyValuePair.Value!), keyValuePair.Key);
                    }

                    foreach (var formFile in multiPartFormCollection.Files)
                    {
                        var memoryStream = new MemoryStream();
                        formFile.OpenReadStream();
                        await formFile.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;
                        var streamContent = new StreamContent(memoryStream);
                        streamContent.Headers.ContentType = new MediaTypeHeaderValue(formFile.ContentType);
                        multipartFormDataContent.Add(streamContent, formFile.Name, formFile.FileName);
                    }

                    message.Content = multipartFormDataContent;
                    break;
                case RequestInfo.UrlEncodedFormDataContentType:
                    var formCollection = (IFormCollection)requestState.RequestInfo.Body;
                    message.Content = new FormUrlEncodedContent(formCollection
                        .ToList()
                        .Select(keyValuePair => new KeyValuePair<string, string>(keyValuePair.Key, keyValuePair.Value.ToString()))
                        .ToArray());
                    break;
                default:
                    await context.Response.SendAsync("Invalid content type", 400);
                    return;
            }
        }

        var watch = Stopwatch.StartNew();
        var httpResponse = await client.SendAsync(message);
        watch.Stop();

        var mediaType = httpResponse.Content.Headers.ContentType?.MediaType;
        dynamic? response;
        if (mediaType is ApplicationJsonMediaType or ApplicationProblemJsonMediaType)
        {
            response = await httpResponse.Content.ReadFromJsonAsync<dynamic>(DefaultSerializerOptions);
        }
        else
        {
            response = await httpResponse.Content.ReadAsStringAsync();
        }

        requestState.ResponseInfo = new ResponseInfo
        {
            Code = (int)httpResponse.StatusCode,
            Value = response,
            ResponseTimeMilliseconds = watch.ElapsedMilliseconds,
            ContentType = httpResponse.Content.Headers.ContentType?.ToString()
        };
        await next(context);
        return;

        bool IgnoreDispatch() => requestState.EndpointDefinition.Meta.TryGetValue(IgnoreDispatchKey, out _);

        string ExtractApiUrl() => $"{requestState.EndpointDefinition.BaseUrl}/{requestState.RequestInfo.Path}{requestState.RequestInfo.QueryString}";

        HttpMethod? MapToHttpMethod() => requestState.RequestInfo.Method switch
        {
            HttpRequestMethods.Get => HttpMethod.Get,
            HttpRequestMethods.Delete => HttpMethod.Delete,
            HttpRequestMethods.Patch => HttpMethod.Patch,
            HttpRequestMethods.Post => HttpMethod.Post,
            HttpRequestMethods.Put => HttpMethod.Put,
            HttpRequestMethods.Head => HttpMethod.Head,
            HttpRequestMethods.Options => HttpMethod.Options,
            _ => null
        };
    }
}