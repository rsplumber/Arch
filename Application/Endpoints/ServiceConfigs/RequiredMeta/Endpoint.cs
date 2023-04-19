using FastEndpoints;

namespace Application.Endpoints.ServiceConfigs.RequiredMeta;

internal sealed class Endpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("service-configs/required-meta");
        // Permissions("arch_service-configs_required-meta");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync(new
        {
            Meta = Array.Empty<string>()
        }, ct);
    }
}