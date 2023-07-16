using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using Core.Middlewares.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Core.Middlewares;

internal sealed class RequestDispatcherMiddleware : ArchMiddleware
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string HttpFactoryName = "arch";
    private const string UserTokenKey = "uid_token";
    private const string ApplicationJsonMediaType = "application/json";
    private const string ApplicationProblemJsonMediaType = "application/problem+json";

    private static readonly JsonSerializerOptions DefaultSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public RequestDispatcherMiddleware(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public override async Task HandleAsync(HttpContext context, RequestDelegate next)
    {
        if (EndpointDefinition is null || RequestInfo is null)
        {
            throw new InvalidRequestException();
        }


        if (IgnoreDispatch())
        {
            await next(context);
            return;
        }

        var client = _httpClientFactory.CreateClient(HttpFactoryName);
        foreach (var header in RequestInfo.Headers)
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (UserIdToken is not null)
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation(UserTokenKey, UserIdToken);
        }


        var message = new HttpRequestMessage(MapToHttpMethod(), ExtractApiUrl());
        if (RequestInfo.Body is not null)
        {
            switch (RequestInfo.ContentType)
            {
                case RequestInfo.ApplicationJsonContentType:
                    message.Content = JsonContent.Create(JsonSerializer.Deserialize<object>(RequestInfo.Body));
                    break;
                case RequestInfo.MultiPartFormData:
                    var multiPartFormCollection = (IFormCollection)RequestInfo.Body;
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
                    var formCollection = (IFormCollection)RequestInfo.Body;
                    message.Content = new FormUrlEncodedContent(formCollection
                        .ToList()
                        .Select(keyValuePair => new KeyValuePair<string, string>(keyValuePair.Key, keyValuePair.Value.ToString()))
                        .ToArray());
                    break;
                default:
                    throw new ContentTypeNotSupportedException();
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

        context.Items[ResponseKey] = new ResponseInfo
        {
            Code = (int)httpResponse.StatusCode,
            Value = response,
            ResponseTimeMilliseconds = watch.ElapsedMilliseconds,
            ContentType = httpResponse.Content.Headers.ContentType?.ToString()
        };
        await next(context);

        string ExtractApiUrl()
        {
            if (EndpointDefinition.BaseUrl is null)
            {
                throw new BaseUrlNotFoundException();
            }

            return $"{EndpointDefinition.BaseUrl}/{RequestInfo.Path}{RequestInfo.QueryString}";
        }

        HttpMethod MapToHttpMethod() => RequestInfo.Method switch
        {
            HttpRequestMethods.Get => HttpMethod.Get,
            HttpRequestMethods.Delete => HttpMethod.Delete,
            HttpRequestMethods.Patch => HttpMethod.Patch,
            HttpRequestMethods.Post => HttpMethod.Post,
            HttpRequestMethods.Put => HttpMethod.Put,
            HttpRequestMethods.Head => HttpMethod.Head,
            HttpRequestMethods.Options => HttpMethod.Options,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}