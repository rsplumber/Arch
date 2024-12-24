namespace Encryption.Tes.Security;

interface IKeyManagement
{
    public Task<string?> ExitsAsync(string cacheKey, CancellationToken cancellationToken = default);

    Task<string> GenerateAsync(string cacheKey, CancellationToken cancellationToken = default);

    Task SaveAsync(string cacheKey, string key, CancellationToken cancellationToken = default);
}