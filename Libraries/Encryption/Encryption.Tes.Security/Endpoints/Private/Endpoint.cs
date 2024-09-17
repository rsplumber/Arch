using Arch.Core.Extensions.Http;
using Arch.Core.Pipeline;
using Encryption.Tes.Security.Domain;
using FastEndpoints;

namespace Encryption.Tes.Security.Endpoints.Key.Private
{
    internal class Endpoint : Endpoint<Request, Response>
    {
        private readonly IKeyManagement _keyManagement;
        private readonly IVersionKeyRepository _versionKeyRepository;

        public Endpoint(IKeyManagement keyManagement, IVersionKeyRepository versionKeyRepository)
        {
            _keyManagement = keyManagement;
            _versionKeyRepository = versionKeyRepository;
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

            var key = await _keyManagement.GenerateAsync(token, ct);
            var cipher = TesEncryption.Decrypt(query.Cipher);
            if (cipher == "InvalidCipher")
            {
                await SendAsync(new Response
                {
                    RequestId = state.RequestInfo.RequestId,
                    RequestDateUtc = state.RequestInfo.RequestDateUtc,
                    Data = cipher
                }, 400, ct);
                return;
            }

            var version = state.RequestInfo.Headers.GetValueOrDefault("version");
            var versionKey = await _versionKeyRepository.FindAsync(int.Parse(version ?? string.Empty), ct);
            if (versionKey is null)
            {
                await SendAsync(new Response
                {
                    RequestId = state.RequestInfo.RequestId,
                    RequestDateUtc = state.RequestInfo.RequestDateUtc,
                    Data = "Invalid version"
                }, 400, ct);
                return;
            }

            var encKey = HashGenerator.GenerateMd5FromString(query.Authorization + versionKey?.Key + query.Cipher);
            var aesEncryption = new AesEncryption(encKey);
            var encryptedBase64 = await aesEncryption.EncryptAsync(key);

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
    [FromHeader("Key")] public string Cipher { get; init; } = default!;

    [FromHeader("Authorization")] public string Authorization { get; init; } = default!;
}