using FastEndpoints;

namespace Arch.Endpoints.EndpointDefinitions.RequiredMeta;

internal sealed class Endpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("endpoint-definitions/required-meta");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await HttpContext.Response.SendOkAsync(new
        {
            Meta = new[]
            {
                new
                {
                    Key = "test"
                }
            }
        }, null, ct);
    }
}