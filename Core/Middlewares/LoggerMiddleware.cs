using DotNetCore.CAP;
using Logging.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Middlewares;

internal sealed class LoggerMiddleware : ArchMiddleware
{
    private readonly IServiceProvider _serviceProvider;
    private const string EventName = "arch.internal.logs";
    private const string LoggingMetaKey = "logging";
    private const string LoggingJustErrorsMetaValue = "error";

    public LoggerMiddleware(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task HandleAsync(HttpContext context, RequestDelegate next)
    {
        if (LoggingDisabled() || IsJustErrorLogging())
        {
            await next(context);
            return;
        }

        using var serviceScope = _serviceProvider.GetService<IServiceScopeFactory>()?.CreateScope();
        var publisher = serviceScope!.ServiceProvider.GetRequiredService<ICapPublisher>();
        await publisher.PublishAsync(EventName, new
        {
            userId = UserId,
            endpoint = EndpointDefinition,
            request = RequestInfo,
            response = ResponseInfo
        });
        await next(context);

        bool LoggingDisabled() => GetMeta(LoggingMetaKey) is null;

        bool IsJustErrorLogging() => GetMeta(LoggingMetaKey) == LoggingJustErrorsMetaValue && ResponseInfo!.Code < 250;
    }
}

internal sealed class ArchInternalLogEventHandler : ICapSubscribe
{
    private readonly IArchLogger _logger;

    public ArchInternalLogEventHandler(IArchLogger logger)
    {
        _logger = logger;
    }

    [CapSubscribe("arch.internal.logs", Group = "arch.core.queue")]
    public async Task HandleAsync(dynamic message)
    {
        await _logger.LogAsync(message);
    }
}