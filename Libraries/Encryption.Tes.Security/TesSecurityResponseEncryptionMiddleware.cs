using System.Text.Json;
using Arch.Core.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Encryption.Tes.Security;

internal sealed class TesSecurityResponseEncryptionMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.RequestState().RequestInfo.Headers.TryGetValue("version", out string value))
        {
            if (int.Parse(value) < 120)
            {
                await next(context);
                return;
            }
        }

        var responseValue = context.RequestState().ResponseInfo.Value;
        context.Items.TryGetValue(TesEncryptionContextKey.EncryptionKey, out var encKey);
        var aesEncryption = new AesEncryption(encKey.ToString());
        var encryptedBase64 = aesEncryption.EncryptStringToBase64(JsonSerializer.Serialize(responseValue));
        context.RequestState().ResponseInfo.Value = encryptedBase64;
        await next(context);
    }
}