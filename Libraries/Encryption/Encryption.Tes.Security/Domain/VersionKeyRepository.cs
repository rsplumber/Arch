using Encryption.Tes.Security.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Encryption.Tes.Security.Domain;

internal sealed class VersionKeyRepository : IVersionKeyRepository
{
    private readonly EncryptionDbContext _encryptionDbContext;

    public VersionKeyRepository(EncryptionDbContext encryptionDbContext)
    {
        _encryptionDbContext = encryptionDbContext;
    }

    public async Task<VersionKey?> FindAsync(int version, CancellationToken cancellationToken = default)
    {
        return await _encryptionDbContext.VersionKey.FirstOrDefaultAsync(key => key.Version == version, cancellationToken);
    }
}