using Microsoft.EntityFrameworkCore;

namespace Encryption.Tes.Security.Domain;

public class VersionKeyRepository : IVersionKeyRepository
{
    private readonly EncryptionDbContext _encryptionDbContext;

    public VersionKeyRepository(EncryptionDbContext encryptionDbContext)
    {
        _encryptionDbContext = encryptionDbContext;
    }

    public async Task<VersionKey?> FindByVersionAsync(int version, CancellationToken cancellationToken = default)
    {
        var versionKey = await _encryptionDbContext.VersionKey.FirstOrDefaultAsync(key => key.Version == version, cancellationToken);
        return versionKey;
    }
}