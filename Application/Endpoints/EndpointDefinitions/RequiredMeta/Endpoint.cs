using FastEndpoints;

namespace Application.Endpoints.EndpointDefinitions.RequiredMeta;

internal sealed class Endpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("endpoint-definitions/required-meta");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync(new
        {
            Meta = new[]
            {
                new
                {
                    Key = "test"
                }
            }
        }, ct);
    }
}