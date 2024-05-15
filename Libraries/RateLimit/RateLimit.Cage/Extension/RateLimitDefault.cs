namespace RateLimit.Cage.Extension;

public static class RateLimitDefault
{
    public static int MaxAllowdRequestInWindow { get; set; }
    public static TimeSpan WindowsSize { get; set; }
    public static int Version { get; set; }
}