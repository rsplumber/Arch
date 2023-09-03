namespace Arch.Logging.Abstractions;

public interface IArchLogger
{
    Task LogAsync(dynamic message);
}