using System.Net.Http.Json;
using Arch.Clerk.Exceptions;
using Core;
using Microsoft.AspNetCore.Http;

namespace Arch.Clerk;

internal class CheckAccountingMiddleware : ArchMiddleware
{
    private readonly IHttpClientFactory _clientFactory;
    private const string HttpClientFactoryKey = "clerk";
    private const string AccountingMetaKey = "accounting";

    public CheckAccountingMiddleware(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public override async Task HandleAsync(HttpContext context, RequestDelegate next)
    {
        if (!HasAccounting())
        {
            await next(context);
            return;
        }

        if (RequestInfo is null)
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
            TariffIdentifier = RequestInfo.Path, RequestInfo.Method
        });
        var response = await httpResponseMessage.Content.ReadFromJsonAsync<dynamic>();
        if (response is null || !response.Result)
        {
            throw new AccountingNoBalanceException();
        }

        await next(context);

        bool HasAccounting() => GetMeta(AccountingMetaKey) is not null;
    }
}