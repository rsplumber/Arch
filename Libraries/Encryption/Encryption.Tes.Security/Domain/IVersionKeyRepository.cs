namespace Encryption.Tes.Security.Domain;

public interface IVersionKeyRepository
{

    Task<VersionKey?> FindByVersionAsync(int version, CancellationToken cancellationToken = default);

}