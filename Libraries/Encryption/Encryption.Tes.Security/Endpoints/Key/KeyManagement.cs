using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;

namespace Encryption.Tes.Security.Endpoints.Key;

internal sealed class KeyManagement:IKeyManagement
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
        // Attempt to retrieve the key from the cache
        var cachedKeyBytes = await _cache.GetAsync(cacheKey, cancellationToken);
        if (cachedKeyBytes != null)
        {
            // Return cached key if found
            var cachedKey = Encoding.UTF8.GetString(cachedKeyBytes);
            Console.WriteLine("GetKeyAsync " + cachedKey);
            return cachedKey;
        }

        // Key not found in cache, generate a new AES key
        using (Aes aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.GenerateKey();
            var aesKeyBase64 = Convert.ToBase64String(aes.Key);

            // Generate MD5 hash of the AES key (MD5 hash is a 32-character hexadecimal string)
            var md5 = HashGenerator.GenerateMd5FromString(aesKeyBase64);

            // Store the MD5 hash in the cache with a 5-minute expiration
            var md5Bytes = Encoding.UTF8.GetBytes(md5);
            var cacheEntryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(50)
            };
            await _cache.SetAsync(cacheKey, md5Bytes, cacheEntryOptions, cancellationToken);

            Console.WriteLine("GetKeyAsync " + md5);
            return md5;
        }
    }


    
}