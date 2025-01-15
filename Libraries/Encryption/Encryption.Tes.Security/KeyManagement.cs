using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;

namespace Encryption.Tes.Security;

internal sealed class KeyManagement : IKeyManagement
{
    private readonly IDistributedCache _cache;

    public KeyManagement(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<string?> ExitsAsync(string cacheKey, CancellationToken cancellationToken)
    {
        var key = await _cache.GetAsync(cacheKey, cancellationToken);
        return key is null ? null : Encoding.UTF8.GetString(key);
    }

    public async Task<string> GenerateAsync(string cacheKey, CancellationToken cancellationToken)
    {
        // Key not found in cache, generate a new AES key
        using (Aes aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.GenerateKey();
            var aesKeyBase64 = Convert.ToBase64String(aes.Key);

            // Generate MD5 hash of the AES key (MD5 hash is a 32-character hexadecimal string)
            var md5 = HashGenerator.GenerateMd5FromString(aesKeyBase64);

            // Store the MD5 hash in the cache with a 5-minute expiration


           // Console.WriteLine("GetKeyAsync " + md5);
            return md5;
        }
    }

    public async Task SaveAsync(string cacheKey, string key, CancellationToken cancellationToken = default)
    {
        var md5Bytes = Encoding.UTF8.GetBytes(key);
        var cacheEntryOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
        };
        await _cache.SetAsync(cacheKey, md5Bytes, cacheEntryOptions, cancellationToken);
    }
}