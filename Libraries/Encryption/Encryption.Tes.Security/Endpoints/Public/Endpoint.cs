using Arch.Core.Extensions.Http;
using Arch.Core.Pipeline;
using Encryption.Tes.Security.Domain;
using FastEndpoints;

namespace Encryption.Tes.Security.Endpoints.Key.Public
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
            Get("key-management/public");
            AllowAnonymous();
            Version(1);
        }

        public override async Task HandleAsync(Request query, CancellationToken ct)
        {
            var state = HttpContext.RequestState();
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


            var key = await _keyManagement.GenerateAsync(query.Cipher, ct);
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

            var encKey = HashGenerator.GenerateMd5FromString(versionKey?.Key + query.Cipher);
            var aesEncryption = new AesEncryption(encKey);
            var encryptedBase64 = await aesEncryption.EncryptAsync(key);
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
        [FromHeader("Key")] public string Cipher { get; init; } = default!;
    }
}