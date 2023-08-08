using DotNetCore.CAP;
using FastEndpoints;
using Logging.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Core.Pipeline;

internal sealed class LoggerMiddleware : IMiddleware
{
    private const string EventName = "arch.internal.logs";
    private const string LoggingMetaKey = "logging";
    private const string LoggingJustErrorsMetaValue = "error";


    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var requestState = context.ProcessorState<RequestState>();
        if (LoggingDisabled() || IsJustErrorLogging())
        {
            await next(context);
            return;
        }

        var publisher = context.Resolve<ICapPublisher>();
        await publisher.PublishAsync(EventName, new
        {
            Meta = requestState.Meta,
            endpoint = requestState.EndpointDefinition,
            request = requestState.RequestInfo,
            response = requestState.ResponseInfo
        });
        await next(context);

        bool LoggingDisabled() => requestState.EndpointDefinition.Meta.TryGetValue(LoggingMetaKey, out _);

        bool IsJustErrorLogging()
        {
            requestState.EndpointDefinition.Meta.TryGetValue(LoggingMetaKey, out var loggingValue);
            return loggingValue == LoggingJustErrorsMetaValue && requestState.ResponseInfo!.Code < 250;
        }
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