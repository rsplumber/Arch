using DotNetCore.CAP;
using Logging.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Middlewares;

internal sealed class LoggerMiddleware : ArchMiddleware
{
    private readonly IServiceProvider _serviceProvider;
    private const string EventName = "arch.internal.logs";

    public LoggerMiddleware(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task HandleAsync(HttpContext context, RequestDelegate next)
    {
        using var serviceScope = _serviceProvider.GetService<IServiceScopeFactory>()?.CreateScope();
        var publisher = serviceScope!.ServiceProvider.GetRequiredService<ICapPublisher>();
        await publisher.PublishAsync(EventName, new
        {
            userId = UserId,
            request = RequestInfo,
            response = ResponseInfo,
            endpoint = EndpointDefinition
        });
        await next(context);
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