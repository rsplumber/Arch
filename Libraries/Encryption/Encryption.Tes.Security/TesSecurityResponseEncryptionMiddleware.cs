using System.Text.Json;
using Arch.Core.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Encryption.Tes.Security;

internal sealed class TesSecurityResponseEncryptionMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var state = context.RequestState();
        state.RequestInfo.Headers.TryGetValue("version", out var value);
        if (string.IsNullOrEmpty(value) || int.Parse(value) < 120 || state.IgnoreDispatch() || state.EndpointDefinition.Meta.ContainsKey("encryptionOff"))
        {
            await next(context).ConfigureAwait(false);
            return;
        }


        if (state.ResponseInfo?.Value is null || state.ResponseInfo.Code > 300)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        var responseValue = state.ResponseInfo?.Value;
        context.Items.TryGetValue(TesEncryptionContextKey.EncryptionKey, out var encKey);
        if (encKey is null) throw new InvalidCipher(message:"InvalidCipher");
        var aesEncryption = new TesSecurityRequestEncryptionMiddleware.AesEncryption(encKey.ToString()!);
        var encryptedBase64 = aesEncryption.EncryptStringToBase64(JsonSerializer.Serialize(responseValue));
        if (context.RequestState().ResponseInfo is null)
        {
            await next(context);
            return;
        }

        context.RequestState().ResponseInfo!.Value = encryptedBase64;
        await next(context);
    }
}