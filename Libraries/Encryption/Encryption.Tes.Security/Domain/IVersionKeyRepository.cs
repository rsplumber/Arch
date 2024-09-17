namespace Encryption.Tes.Security.Domain;

internal interface IVersionKeyRepository
{
    Task<VersionKey?> FindAsync(int version, CancellationToken cancellationToken = default);
}