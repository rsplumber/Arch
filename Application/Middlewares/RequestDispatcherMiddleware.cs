using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Application.Exceptions;
using Core.Library;
using Microsoft.IdentityModel.Tokens;

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
        if (RequestInfo.Headers is not null)
        {
            foreach (var header in RequestInfo.Headers)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        var userId = context.Items[UserIdKey];
        if (userId is not null)
        {
            var userToken = GenerateUserToken();
            client.DefaultRequestHeaders.TryAddWithoutValidation(UserTokenKey, userToken);
        }

        object? requestBody = null;
        if (RequestInfo.Body is not null)
        {
            requestBody = JsonSerializer.Deserialize<object>(RequestInfo.Body);
        }

        // HttpRequestMessage requestMessage = this.CreateRequestMessage(HttpMethod.Put, ApiUrl());
        // requestMessage.Content = content;
        // client.SendAsync(new HttpRequestMessage(requestMessage)

        var httpResponse = RequestInfo.Method switch
        {
            HttpRequestMethods.Get => await client.GetAsync(ApiUrl()),
            HttpRequestMethods.Delete => await client.DeleteAsync(ApiUrl()),
            HttpRequestMethods.Patch => await client.PatchAsJsonAsync(ApiUrl(), requestBody),
            HttpRequestMethods.Post => await client.PostAsJsonAsync(ApiUrl(), requestBody),
            HttpRequestMethods.Put => await client.PutAsJsonAsync(ApiUrl(), requestBody),
            _ => throw new ArgumentOutOfRangeException()
        };

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

            return $"{EndpointDefinition.BaseUrl}/{RequestInfo.Path}";
        }

        string GenerateUserToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(RequestInfo.Headers.First(pair => pair.Key == "service_secret").Value);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", userId.ToString()!) }),
                Expires = DateTime.UtcNow.AddSeconds(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}