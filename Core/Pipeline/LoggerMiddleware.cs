﻿using Arch.Core.Extensions;
using Arch.Logging.Abstractions;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Http;

namespace Arch.Core.Pipeline;

internal sealed class LoggerMiddleware : IMiddleware
{
    private const string EventName = "arch.internal.logs";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var requestState = context.RequestState();
        if (!requestState.EndpointDefinition.Logging.Enabled || IsJustErrorLogging())
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        _ = SendLogsAsync().ConfigureAwait(false);

        await next(context).ConfigureAwait(false);
        return;

        bool IsJustErrorLogging() => requestState.EndpointDefinition.Logging.JustError && requestState.ResponseInfo!.Code < 250;

        Task SendLogsAsync()
        {
            var eventBus = context.EventBus();
            return eventBus.PublishAsync(EventName, new
            {
                endpoint = requestState.EndpointDefinition,
                request = requestState.RequestInfo,
                response = requestState.ResponseInfo
            });
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