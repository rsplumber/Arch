using FastEndpoints;

namespace Arch.Endpoints.Get;

internal sealed class Endpoint : EndpointWithoutRequest
{
    private readonly IHttpClient _httpClient;


    public Endpoint(IHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public override void Configure()
    {
        Get("");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var path = HttpContext.Request.Path;
        var response = await _httpClient.GetRequestAsync<object>("", "");
        await SendOkAsync(response, ct);
    }
}

internal sealed class EndpointSummary : Summary<Endpoint>
{
    public EndpointSummary()
    {
        Summary = "Getting Address string value by postcode";
        Description = "Getting Address string value by postcode";
        Response<object>(200, "AddressString was successfully returned");
    }
}