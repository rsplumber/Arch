using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;

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

    public CheckAccountingMiddleware(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        dynamic endpointDefinition = context.Items[ArchEndpointDefinitionKey]!;

        if (!HasAccounting())
        {
            await next(context);
            return;
        }

        dynamic? requestInfo = context.Items[RequestInfoKey];
        if (requestInfo is null)
        {
            await next(context);
            return;
        }

        var client = _clientFactory.CreateClient(HttpClientFactoryKey);
        var userId = context.Items[UserIdKey];
        if (userId is null)
        {
            throw new AccountingUserNotFoundException();
        }

        var httpResponseMessage = await client.PostAsJsonAsync($"{ClerkAccountingSettings.BaseUrl}/api/v1/accounts/{userId}/tariff/pay", new
        {
            TariffIdentifier = requestInfo.Path, requestInfo.Method
        });
        var response = await httpResponseMessage.Content.ReadFromJsonAsync<dynamic>();
        if (response is null || !response.Result)
        {
            throw new AccountingNoBalanceException();
        }

        await next(context);

        bool HasAccounting()
        {
            dynamic? accounting = null;
            foreach (var meta in endpointDefinition.Meta)
            {
                if (meta.Key != AccountingMetaKey) continue;
                accounting = meta;
                break;
            }

            return accounting is not null && accounting.Value == TrueKey;
        }
    }
}