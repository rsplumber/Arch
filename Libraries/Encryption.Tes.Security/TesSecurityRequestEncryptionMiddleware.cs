using Microsoft.AspNetCore.Http;

namespace Encryption.Tes.Security;

internal sealed class TesSecurityRequestEncryptionMiddleware : IMiddleware
{



    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        Console.WriteLine("this is request encryption test");
        await next(context);
    }
}