namespace Logging.Abstractions;

public interface IArchLogger
{
    Task LogAsync(dynamic message);
}