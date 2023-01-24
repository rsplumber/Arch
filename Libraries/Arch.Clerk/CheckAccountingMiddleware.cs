using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;

namespace Arch.Clerk;

internal class CheckAccountingMiddleware : IMiddleware
{
    private const string HttpClientFactoryKey = "clerk";
    private const string RequestInfoKey = "request_info";
    private const string ArchEndpointDefinitionKey = "arch_endpoint_definition";
    private readonly IHttpClientFactory _clientFactory;
    private const string TrueKey = "true";
    private const string AccountingMetaKey = "accounting";

    public CheckAccountingMiddleware(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        dynamic info = context.Items[RequestInfoKey];
        dynamic arch = context.Items[ArchEndpointDefinitionKey];
        dynamic? accounting = null;
        foreach (var meta in arch.Meta)
        {
            var id = meta.Id;
            Console.WriteLine(id);
            if (id! != AccountingMetaKey) continue;
            accounting = meta;
            break;
        }


        if (accounting is null || accounting.Value != TrueKey) return;

        var client = _clientFactory.CreateClient(HttpClientFactoryKey);
        var httpResponseMessage = await client.PostAsJsonAsync("http://192.168.70.117:5140/api/v1/accounts/dd7dd1a9-0db6-45b3-93c6-2e2026f2d050/tariff/pay", new
        {
            info.Path
        });
        if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
        {
            await next(context);
        }

        throw new Exception("No balance");
    }
}