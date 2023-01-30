using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Arch.Clerk;

internal class CheckAccountingMiddleware : IMiddleware
{
    private readonly IHttpClientFactory _clientFactory;
    private const string HttpClientFactoryKey = "clerk";
    private const string RequestInfoKey = "request_info";
    private const string ArchEndpointDefinitionKey = "arch_endpoint_definition";
    private const string TrueKey = "true";
    private const string AccountingMetaKey = "accounting";
    private const string UserIdKey = "user_id";
    private readonly string _clerkBaseUrl;

    public CheckAccountingMiddleware(IHttpClientFactory clientFactory, IConfiguration configuration)
    {
        _clientFactory = clientFactory;
        _clerkBaseUrl = configuration.GetSection("Clerk:BaseUrl").Value ?? throw new Exception("Enter Clerk:BaseUrl in appsettings.json");
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        dynamic archEndpointDefinition = context.Items[ArchEndpointDefinitionKey]!;
        dynamic? accounting = null;
        foreach (var meta in archEndpointDefinition.Meta)
        {
            var id = meta.Key;
            if (id! != AccountingMetaKey) continue;
            accounting = meta;
            break;
        }


        if (accounting is null || accounting.Value != TrueKey)
        {
            await next(context);
            return;
        }

        dynamic? info = context.Items[RequestInfoKey];
        if (info is null)
        {
            await next(context);
            return;
        }

        var client = _clientFactory.CreateClient(HttpClientFactoryKey);
        var userId = context.Items[UserIdKey] as string;
        var httpResponseMessage = await client.PostAsJsonAsync($"{_clerkBaseUrl}/api/v1/accounts/{userId}/tariff/pay", new
        {
            TariffIdentifier = info.Path
        });
        var response = await httpResponseMessage.Content.ReadFromJsonAsync<ApiResponse>();
        if (response is null || !response.Result) throw new NoBalanceException();
        await next(context);
    }
}

internal class ApiResponse
{
    public bool Result { get; set; }
}