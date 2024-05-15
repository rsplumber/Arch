namespace RateLimit.Cage.Extension;

public static class RemindedTime
{
    public static string Calculate(DateTime startDate, TimeSpan addedTime)
    {
        var endDate = startDate + addedTime;
        var totalSeconds = (int)(endDate - DateTime.UtcNow).TotalSeconds;
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        return $"{minutes}:{seconds}";
    }
}