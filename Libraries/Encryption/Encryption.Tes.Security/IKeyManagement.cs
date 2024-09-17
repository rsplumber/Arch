namespace Encryption.Tes.Security.Endpoints.Key;

interface IKeyManagement
{
    public Task<string?> ExitsAsync(string cacheKey, CancellationToken cancellationToken = default);
    Task<string> GenerateAsync(string cacheKey, CancellationToken cancellationToken = default);
} 