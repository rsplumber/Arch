using FastEndpoints;
using Microsoft.Extensions.Options;

namespace Arch.Endpoints.Post.Addresses.Text;

internal sealed class Endpoint : Endpoint<Request, Response>
{
    private readonly IHttpClient _httpClient;
    private readonly PostOptions _postOptions;
    private const string ApiUrl = "addresses/{postcodes}";


    public Endpoint(IHttpClient httpClient, IOptions<PostOptions> postOptions)
    {
        _httpClient = httpClient;
        _postOptions = postOptions.Value ?? throw new ArgumentNullException(nameof(postOptions), "enter Post options");
    }

    public override void Configure()
    {
        Get(ApiUrl);
        Permissions("post_address_detail_by_postcode_v2");
        Version(1);
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var response = await _httpClient.PostRequestAsync<Request, Response>(_postOptions.BaseUrl + ApiUrl, req);

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
        Summary = "Getting Address detail by postcode";
        Description = "Getting Address detail by postcode";
        Response<Response>(200, "AddressDetail was successfully returned");
    }
}

internal sealed record Response
{
    public string Province { get; init; } = default!;

    public string TownShip { get; init; } = default!;

    public string Zone { get; init; } = default!;

    public string? Village { get; init; }

    public string LocalityType { get; init; } = default!;

    public string LocalityName { get; init; } = default!;

    public int LocalityCode { get; init; } = default!;

    public string? SubLocality { get; init; }

    public string Street { get; init; } = default!;

    public string? Street2 { get; init; }

    public int HouseNumber { get; init; } = default!;

    public string? Floor { get; init; }

    public string? SideFloor { get; init; }

    public string? BuildingName { get; init; }

    public string? Description { get; init; }
}