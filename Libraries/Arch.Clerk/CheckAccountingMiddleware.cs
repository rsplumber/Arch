using System.Net;
using System.Net.Http.Json;
using Arch.Clerk.Exceptions;
using Core;
using Core.Middlewares.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Arch.Clerk;

internal class CheckAccountingMiddleware : ArchMiddleware
{
    private readonly IHttpClientFactory _clientFactory;
    private const string HttpClientFactoryKey = "clerk";
    private const string AccountingMetaKey = "accounting";
    private const string PayApi = "api/v1/pay";


    public CheckAccountingMiddleware(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public override async Task HandleAsync(HttpContext context, RequestDelegate next)
    {
        if (RequestInfo is null)
        {
            throw new InvalidRequestException();
        }

        var accountingIdentifier = GetMeta(AccountingMetaKey);
        if (!HasAccounting())
        {
            await next(context);
            return;
        }

        if (context.Items[UserIdTokenKey] is not string userId)
        {
            throw new AccountingUserNotFoundException();
        }

        var client = _clientFactory.CreateClient(HttpClientFactoryKey);
        var httpResponse = await client.PostAsJsonAsync(PayApi, new
        {
            UserId = userId,
            TariffIdentifier = accountingIdentifier
        });
        if (httpResponse is null || httpResponse.StatusCode != HttpStatusCode.OK)
        {
            throw new AccountingNoBalanceException();
        }

        await next(context);

        bool HasAccounting() => accountingIdentifier is not null;
    }
}