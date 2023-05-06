using DotNetCore.CAP;

namespace Core.Logs;

internal class ArcLoggerEventSender : IArcLogger
{
    private readonly ICapPublisher _eventBus;
    private const string EventName = "arch.logs";

    public ArcLoggerEventSender(ICapPublisher eventBus)
    {
        _eventBus = eventBus;
    }

    public Task LogAsync(dynamic message)
    {
        return _eventBus.PublishAsync(EventName, message);
    }
}