using System.Text.Json;
using Arch.Core.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Encryption.Tes.Security;

internal sealed class TesSecurityResponseEncryptionMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var responseValue = context.RequestState().ResponseInfo.Value;
        context.Items.TryGetValue("tes_encryption_key", out var encKey);

        var key = string.Empty;
        if (context.Request.Headers.TryGetValue("key", out var cipheredKey))
        {
            key = Encryption.Decrypt(cipheredKey.FirstOrDefault());
        }
        
        var apkMd5 = "a60c6906f98dc4aad77585f5b314e54a";
        var aesEncryption = new AesEncryption(encKey.ToString());
        var encryptedBase64 = aesEncryption.EncryptStringToBase64(JsonSerializer.Serialize(responseValue));
        context.RequestState().ResponseInfo.Value = encryptedBase64;
        await next(context);
    }
}