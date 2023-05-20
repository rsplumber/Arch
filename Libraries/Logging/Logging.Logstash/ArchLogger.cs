using DotNetCore.CAP;
using Logging.Abstractions;

namespace Logging.Logstash;

internal sealed class ArchLogger : IArchLogger
{
    private readonly ICapPublisher _eventBus;
    private const string EventName = "arch.logs";

    public ArchLogger(ICapPublisher eventBus)
    {
        _eventBus = eventBus;
    }

    public Task LogAsync(dynamic message)
    {
        return _eventBus.PublishAsync(EventName, message);
    }
}