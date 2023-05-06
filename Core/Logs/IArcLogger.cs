namespace Core.Logs;

public interface IArcLogger
{
    Task LogAsync(dynamic message);
}