using System.Text.Json;
using Arch.Core.Extensions.Http;
using Arch.Core.Pipeline;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Encryption.Tes.Security;

internal sealed class TesSecurityResponseEncryptionMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var state = context.RequestState();
        state.RequestInfo.Headers.TryGetValue("version", out var version);
        state.EndpointDefinition.Meta.TryGetValue("encryption", out var encryptionMeta);
        if (string.IsNullOrEmpty(version) ||
            int.Parse(version) < 120 ||
            state.IgnoreDispatch() ||
            (encryptionMeta is not null && encryptionMeta == "disable"))
        {
            await next(context).ConfigureAwait(false);
            return;
        }


        if (IgnoreEmptyOrErrorResponse())
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        var responseValue = state.ResponseInfo?.Value;
        context.Items.TryGetValue(TesEncryptionContextKey.EncryptionKey, out var encryptionKey);
        if (encryptionKey is null)
        {
            await context.Response.SendAsync(new Response
            {
                RequestId = state.RequestInfo.RequestId,
                RequestDateUtc = state.RequestInfo.RequestDateUtc,
                Data = new
                {
                    message = "InvalidKey",
                    clientMessage = string.Empty
                }
            }, 400);
            return;
        }

        var aesEncryption = new AesEncryption((string)encryptionKey);
        var encryptedBase64 = await aesEncryption.EncryptAsync(JsonSerializer.Serialize(responseValue));
        if (context.RequestState().ResponseInfo is null)
        {
            await next(context);
            return;
        }

        context.RequestState().ResponseInfo!.Value = encryptedBase64;
        await next(context);

        return;

        bool IgnoreEmptyOrErrorResponse() => state.ResponseInfo?.Value is null || state.ResponseInfo.Code > 300;
    }
}