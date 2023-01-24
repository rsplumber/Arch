using FastEndpoints;

namespace Application;

internal sealed class HealthEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("health");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync("healthy", ct);
    }
}