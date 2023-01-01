using FastEndpoints;
using Microsoft.Extensions.Options;

namespace Arch.Endpoints.Post.Addresses.Default;

internal sealed class Endpoint : Endpoint<Request, Response>
{
    private readonly IHttpClient _httpClient;
    private readonly PostOptions _postOptions;


    public Endpoint(IHttpClient httpClient, IOptions<PostOptions> postOptions)
    {
        _httpClient = httpClient;
        _postOptions = postOptions.Value ?? throw new ArgumentNullException(nameof(postOptions), "enter Post options");
    }

    public override void Configure()
    {
        Get("addresses/{postcodes}/value");
        Permissions("post_address_value_by_postcode_v2");
        Version(1);
        Tags("Post");
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var path = HttpContext.Request.Path;
        var response = await _httpClient.PostRequestAsync<Request, Response>(_postOptions.BaseUrl + path, req);

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