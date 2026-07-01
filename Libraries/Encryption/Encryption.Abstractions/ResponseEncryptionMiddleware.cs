using System.Text.Json;
using Arch.Core.Extensions.Http;
using Arch.Core.Pipeline;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Encryption.Abstractions;

internal sealed class ResponseEncryptionMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var state = context.RequestState();

        if (!state.Endpoint.Meta.TryGetValue("encrypted", out var providerName) ||
            string.IsNullOrEmpty(providerName) ||
            state.IgnoreDispatch() ||
            state.ResponseInfo?.Value is null ||
            state.ResponseInfo.Code > 300)
        {
            await next(context);
            return;
        }

        var handler = context.RequestServices.GetServices<IEncryptionHandler>()
            .FirstOrDefault(h => h.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));

        if (handler is null)
        {
            await context.Response.SendAsync(new Response
            {
                RequestId = state.RequestInfo.RequestId,
                RequestDateUtc = state.RequestInfo.RequestDateUtc,
                Data = new { message = "UnknownEncryptionProvider", clientMessage = string.Empty }
            }, 400);
            return;
        }

        var encryptedValue = await handler.EncryptAsync(JsonSerializer.Serialize(state.ResponseInfo.Value), context);
        state.ResponseInfo.Value = encryptedValue;

        await next(context);
    }
}
