using Arch.Core.Extensions.Http;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Http;

namespace Arch.Logging.Abstractions;

public sealed class LoggerMiddleware : IMiddleware
{
    private const string EventName = "arch.internal.logs";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.Request.EnableBuffering();
        var requestState = context.RequestState();
        if (requestState.EndpointDefinition.Logging.Disabled)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        if (requestState.ResponseInfo is not null)
        {
            _ = SendLogsAsync().ConfigureAwait(false);
        }

        await next(context).ConfigureAwait(false);
        return;

        Task SendLogsAsync()
        {
            var eventBus = context.EventBus();
            object logData;
            var endpointData = new
            {
                requestState.EndpointDefinition.Id,
                requestState.EndpointDefinition.Endpoint,
                requestState.EndpointDefinition.Method,
                Service = new
                {
                    requestState.EndpointDefinition.ServiceConfig.Id,
                    requestState.EndpointDefinition.ServiceConfig.Name,
                    requestState.EndpointDefinition.ServiceConfig.BaseUrls,
                }
            };
            if (requestState.EndpointDefinition.Logging.Informal)
            {
                logData = new
                {
                    endpoint = endpointData,
                    request = requestState.RequestInfo,
                    response = requestState.ResponseInfo
                };
            }
            else
            {
                /*logData = new
                {
                    endpoint = endpointData,
                    request = requestState.RequestInfo.RequestId,
                    response = requestState.ResponseInfo?.Code
                };*/
                
                logData = new
                {
                    endpoint = endpointData,
                    request = requestState.RequestInfo,
                    response = requestState.ResponseInfo
                };
            }

            return eventBus.PublishAsync(EventName, logData);
        }
    }
}

public sealed class ArchInternalLogEventHandler : ICapSubscribe
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