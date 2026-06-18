namespace RateLimit.ArchLimit.Models;

public static class ArchLimitDefaults
{
    public static int MaxRequests { get; set; }
    public static TimeSpan Window { get; set; }
    public static int Version { get; set; }
}
