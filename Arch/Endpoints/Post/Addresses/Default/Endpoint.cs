using FastEndpoints;

namespace Arch.Endpoints.Post.Addresses.Default;

internal sealed class Endpoint : Endpoint<Request, Response>
{
    private readonly IHttpClient _httpClient;
    private const string ApiUrl = "identity/national-code/check";
    private const string ServiceName = "Post";


    public Endpoint(IHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public override void Configure()
    {
        Get("addresses/{postcodes}/value");
        Permissions("post_address_value_by_postcode_v2");
        Version(1);
        Group<PostGroup>();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var path = HttpContext.Request.Path;
        var response = await _httpClient.PostRequestAsync<Request, Response>(ServiceName, ApiUrl, req);

        await SendOkAsync(response, ct);
    }
}

internal sealed record Request
{
    public List<string> PostCodes { get; init; } = default!;
}

internal sealed class EndpointSummary : Summary<Endpoint>
{
    public EndpointSummary()
    {
        Summary = "Getting Address string value by postcode";
        Description = "Getting Address string value by postcode";
        Response<Response>(200, "AddressString was successfully returned");
    }
}

internal sealed record Response
{
    public string Value { get; init; } = default!;

    public string Postcode { get; init; } = default!;
}