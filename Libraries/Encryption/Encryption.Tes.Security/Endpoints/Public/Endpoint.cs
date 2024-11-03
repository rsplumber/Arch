using Arch.Core.Extensions.Http;
using Arch.Core.Pipeline;
using FastEndpoints;

namespace Encryption.Tes.Security.Endpoints.Public
{
    internal class Endpoint : Endpoint<Request, Response>
    {
        private readonly IKeyManagement _keyManagement;

        public Endpoint(IKeyManagement keyManagement)
        {
            _keyManagement = keyManagement;
        }


        public override void Configure()
        {
            Get("key-management/public");
            AllowAnonymous();
            Version(1);
        }

        public override async Task HandleAsync(Request query, CancellationToken ct)
        {
            var state = HttpContext.RequestState();
            var key = TesEncryption.Decrypt(query.Key);
            if (key == "InvalidCipher")
            {
                await SendAsync(new Response
                {
                    RequestId = state.RequestInfo.RequestId,
                    RequestDateUtc = state.RequestInfo.RequestDateUtc,
                    Data = "InvalidCipher"
                }, 400, ct);
                return;
            }


            var cacheKey = await _keyManagement.ExitsAsync(key, ct);
            if (cacheKey is null)
            {
                cacheKey = await _keyManagement.GenerateAsync(key, ct);
                await _keyManagement.SaveAsync(key, cacheKey, ct);
            }
            

            var encKey = HashGenerator.GenerateMd5FromString(key);
            var aesEncryption = new AesEncryption(encKey);
            var encryptedBase64 = await aesEncryption.EncryptAsync(cacheKey);
            await SendOkAsync(new Response
            {
                RequestId = HttpContext.RequestState().RequestInfo.RequestId,
                RequestDateUtc = HttpContext.RequestState().RequestInfo.RequestDateUtc,
                Data = encryptedBase64
            }, ct);
        }
    }

    internal sealed class Request
    {
        [FromHeader("Key")] public string Key { get; init; } = default!;
    }
}