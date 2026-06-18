using System.Text;
using Arch.Core.Extensions.Http;
using Arch.Core.Pipeline;
using Arch.Core.Pipeline.Models;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Encryption.Abstractions;

internal sealed class RequestEncryptionMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var state = context.RequestState();

        if (!state.EndpointDefinition.Meta.TryGetValue("encrypted", out var providerName) ||
            string.IsNullOrEmpty(providerName) ||
            state.IgnoreDispatch())
        {
            await next(context);
            return;
        }

        if (context.Request.ContentType() is RequestInfo.UrlEncodedFormDataContentType or RequestInfo.MultiPartFormData ||
            !context.Request.HasBody())
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

        context.Request.EnableBuffering();
        var reader = new StreamReader(context.Request.Body);
        var encryptedBody = await reader.ReadToEndAsync();
        if (context.Request.Body.CanSeek) context.Request.Body.Seek(0, SeekOrigin.Begin);

        string decryptedBody;
        try
        {
            decryptedBody = await handler.DecryptAsync(encryptedBody, context);
        }
        catch
        {
            await context.Response.SendAsync(new Response
            {
                RequestId = state.RequestInfo.RequestId,
                RequestDateUtc = state.RequestInfo.RequestDateUtc,
                Data = "IncorrectData"
            }, 400);
            return;
        }

        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(decryptedBody));
        context.Request.ContentType = RequestInfo.ApplicationJsonContentType;
        state.RequestInfo.Headers.Remove("Content-Type");
        state.RequestInfo.Headers.Add("Content-Type", RequestInfo.ApplicationJsonContentType);

        await next(context);
    }
}
