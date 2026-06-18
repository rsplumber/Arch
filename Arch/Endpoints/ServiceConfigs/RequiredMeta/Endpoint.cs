using FastEndpoints;

namespace Arch.Endpoints.ServiceConfigs.RequiredMeta;

internal sealed class Endpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("service-configs/required-meta");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await HttpContext.Response.SendOkAsync(new
        {
            Meta = Array.Empty<string>()
        }, null, ct);
    }
}