using Arch.Responses;
using FastEndpoints;

namespace Arch.Endpoints.Post.Addresses.Text;

internal sealed class Endpoint : Endpoint<Request, BaseResponse<Response>>
{
    private const string ApiUrl = "identity/national-code/check";
    private const string ServiceName = "Post";
    private readonly IHttpClient _httpClient;


    public Endpoint(IHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public override void Configure()
    {
        Get("addresses/{postcodes}");
        Permissions("post_address_detail_by_postcode_v2");
        Version(1);
        Group<PostGroup>();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var path = HttpContext.Request.Path;
        var response = await _httpClient.GetRequestAsync<Response>(ServiceName, ApiUrl);


        await SendOkAsync(ResponseFactory<Response>.Create(response), ct);
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