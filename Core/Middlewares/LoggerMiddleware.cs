using Core.Logs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Middlewares;

internal sealed class LoggerMiddleware : ArchMiddleware
{
    private readonly IServiceProvider _serviceProvider;

    public LoggerMiddleware(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task HandleAsync(HttpContext context, RequestDelegate next)
    {
        using var serviceScope = _serviceProvider.GetService<IServiceScopeFactory>()?.CreateScope();
        var logger = serviceScope!.ServiceProvider.GetRequiredService<IArcLogger>();
        await logger.LogAsync(new
        {
            userId = UserId,
            request = RequestInfo,
            response = ResponseInfo,
            endpoint = EndpointDefinition
        });
        await next(context);
    }
}