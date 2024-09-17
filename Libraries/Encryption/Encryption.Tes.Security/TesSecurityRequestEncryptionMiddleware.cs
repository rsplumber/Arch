using System.Text;
using Arch.Core.Extensions.Http;
using Arch.Core.Pipeline;
using Arch.Core.Pipeline.Models;
using Encryption.Tes.Security.Endpoints.Key;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Encryption.Tes.Security;

internal sealed class TesSecurityRequestEncryptionMiddleware : IMiddleware
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

        var requestInfo = context.RequestState().RequestInfo;

        context.Request.Headers.TryGetValue("key", out var apiKey);
        var seed = CalculateSeed();
        if (seed is null)
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

        var authorizationToken = GetAuthorizationToken();

        var encryptedRequest = await ReadRequestBodyAsync();

        var keyManagement = context.RequestServices.GetRequiredService<IKeyManagement>();

        string? encryptionKey;
        if (authorizationToken.Length == 0)
        {
            var cipherKey = apiKey.FirstOrDefault() ?? string.Empty;
            encryptionKey = await keyManagement.ExitsAsync(cipherKey);
        }
        else
        {
            encryptionKey = await keyManagement.ExitsAsync(authorizationToken);
        }

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
            }, 460);
            return;
        }

        context.Items.Add(TesEncryptionContextKey.EncryptionKey, encryptionKey);

        if (context.Request.ContentType() is
            RequestInfo.UrlEncodedFormDataContentType or
            RequestInfo.MultiPartFormData)
        {
            await next(context);
            return;
        }


        if (!context.Request.HasBody())
        {
            await next(context);
            return;
        }

        var aesEncryption = new AesEncryption(encryptionKey);
        string decryptedText;
        try
        {
            decryptedText = await aesEncryption.DecryptAsync(encryptedRequest);
        }
        catch (Exception)
        {
            await context.Response.SendAsync(new Response
            {
                RequestId = state.RequestInfo.RequestId,
                RequestDateUtc = state.RequestInfo.RequestDateUtc,
                Data = "IncorrectData",
            }, 400);
            return;
        }

        RefillRequestData();

        await next(context);

        return;

        string GetAuthorizationToken()
        {
            return requestInfo.Headers.TryGetValue("Authorization", out var token) ? token : string.Empty;
        }


        string? CalculateSeed()
        {
            var cipherKey = apiKey.FirstOrDefault() ?? string.Empty;
            return cipherKey.Contains("InvalidCipher") ? null : TesEncryption.Decrypt(cipherKey);
        }

        async Task<string> ReadRequestBodyAsync()
        {
            context.Request.EnableBuffering();
            var reader = new StreamReader(context.Request.Body);
            var requestBody = await reader.ReadToEndAsync();
            if (context.Request.Body.CanSeek) context.Request.Body.Seek(0, SeekOrigin.Begin);
            return requestBody;
        }

        void RefillRequestData()
        {
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(decryptedText));
            context.Request.ContentType = RequestInfo.ApplicationJsonContentType;
            context.RequestState().RequestInfo.Headers.Remove("Content-Type");
            context.RequestState().RequestInfo.Headers.Add("Content-Type", RequestInfo.ApplicationJsonContentType);
        }
    }
}