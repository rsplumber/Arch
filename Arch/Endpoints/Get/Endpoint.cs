using FastEndpoints;

namespace Arch.Endpoints.Get;

internal sealed class Endpoint : EndpointWithoutRequest
{
    

    public override void Configure()
    {
        Get();
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // var path = HttpContext.Request.Path;
        // var response = await _httpClient.GetRequestAsync<object>("", "");
        // await SendOkAsync(response, ct);
    }
}
