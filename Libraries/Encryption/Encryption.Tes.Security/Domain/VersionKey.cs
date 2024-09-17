namespace Encryption.Tes.Security.Domain;

public class VersionKey
{
    public string Key { get; set; } = default!;

    public int Version { get; set; }

    public DateTime CreateDateUtc { get; private set; } = DateTime.UtcNow;
}