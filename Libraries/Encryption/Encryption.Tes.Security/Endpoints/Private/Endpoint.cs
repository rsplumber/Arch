using Arch.Core.Extensions.Http;
using Arch.Core.Pipeline;
using FastEndpoints;

namespace Encryption.Tes.Security.Endpoints.Key.Private
{
    internal class Endpoint : Endpoint<Request, Response>
    {
        private readonly IKeyManagement _keyManagement;


        public Endpoint(IKeyManagement keyManagement )
        {
            _keyManagement = keyManagement;
        }


        public override void Configure()
        {
            Get("key-management/private");
            AllowAnonymous();
            Version(1);
        }

        public override async Task HandleAsync(Request query, CancellationToken ct)
        {
            var state = HttpContext.RequestState();
            var token = query.Authorization;
            if (token.Length == 0)
            {
                await SendUnauthorizedAsync(ct);
                return;
            }


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

            var cacheKey = await _keyManagement.ExitsAsync(token, ct);
            if (cacheKey is null)
            {
                cacheKey = await _keyManagement.GenerateAsync(token, ct);
                await _keyManagement.SaveAsync(token, cacheKey, ct);
            }


            var encKey = HashGenerator.GenerateMd5FromString(query.Authorization + key);
            var aesEncryption = new AesEncryption(encKey);
            var encryptedBase64 = await aesEncryption.EncryptAsync(cacheKey);
            await _keyManagement.SaveAsync(token, cacheKey, ct);

            var res = new Response
            {
                RequestId = HttpContext.RequestState().RequestInfo.RequestId,
                RequestDateUtc = HttpContext.RequestState().RequestInfo.RequestDateUtc,
                Data = encryptedBase64
            };
            await SendOkAsync(res, ct);
        }
    }
}

internal sealed class Request
{
    [FromHeader("Key")] public string Key { get; init; } = default!;

    [FromHeader("Authorization")] public string Authorization { get; init; } = default!;
}