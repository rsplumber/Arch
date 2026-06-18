namespace RateLimit.ArchLimit.Models;

public class RateLimitEntry
{
    public int Count { get; set; }
    public DateTime LastAccess { get; set; }

    public RateLimitEntry(DateTime lastAccess)
    {
        Count = 0;
        LastAccess = lastAccess;
    }
}
