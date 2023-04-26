using System.Text.Json;
using Application.Middlewares.Exceptions;
using Core.Library;

namespace Application.Middlewares;

internal sealed class RequestDispatcherMiddleware : ArchMiddleware
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string HttpFactoryName = "arch";
    private const string UserTokenKey = "uid_token";

    public RequestDispatcherMiddleware(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public override async Task HandleAsync(HttpContext context, RequestDelegate next)
    {
        if (EndpointDefinition is null || RequestInfo is null) return;


        if (IgnoreDispatch())
        {
            await next(context);
            return;
        }

        var client = _httpClientFactory.CreateClient(HttpFactoryName);
        client.DefaultRequestHeaders.Clear();
        foreach (var header in RequestInfo.Headers)
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (context.Items[UserIdKey] is string userId)
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation(UserTokenKey, userId);
        }


        var message = new HttpRequestMessage(MapToHttpMethod(), ApiUrl());
        if (RequestInfo.Body is not null)
        {
            message.Content = RequestInfo.ContentType switch
            {
                RequestInfo.ApplicationJsonContentType => JsonContent.Create(JsonSerializer.Deserialize<object>(RequestInfo.Body)),
                RequestInfo.FormDataContentType => new StringContent(RequestInfo.Body),
                _ => message.Content
            };
        }

        var httpResponse = await client.SendAsync(message);
        var response = await httpResponse.Content.ReadAsStringAsync();
        context.Items[ResponseKey] = new ResponseInfo
        {
            Code = (int)httpResponse.StatusCode,
            Value = response
        };
        await next(context);

        string ApiUrl()
        {
            if (EndpointDefinition.BaseUrl is null)
            {
                throw new BaseUrlNotfoundException();
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