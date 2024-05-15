namespace RateLimit.Cage.Extension;

public class LimitCondition
{
    public string IdentifierInRequestBody { get; set; }
    public TimeSpan WindowsSize { get; set; }
    public int MaxAllowedRequestInWindow { get; set; }
    public string BriefURL { get; set; }
}

public static class GlobaConditions
{
    public static LimitCondition Values()
    {
        return new LimitCondition()
        {
            BriefURL = "global",
            IdentifierInRequestBody = "username",
            MaxAllowedRequestInWindow = 5,
            WindowsSize = TimeSpan.FromMinutes(1),
        };
    }
}