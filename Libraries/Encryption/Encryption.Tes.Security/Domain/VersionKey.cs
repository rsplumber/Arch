namespace Encryption.Tes.Security.Domain;

public class VersionKey : Entity
{
    
    public string Key { get;  set; } = default!;

    public int Version { get;  set; } = default!;

    public DateTime CreateDateUtc { get; private set; } = DateTime.UtcNow;
    
}