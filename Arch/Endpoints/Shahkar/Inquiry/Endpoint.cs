using FastEndpoints;

namespace Arch.Endpoints.Shahkar.Inquiry;

internal sealed class Endpoint : Endpoint<Request, Response?>
{
    private readonly IHttpClient _httpClient;
    private const string ApiUrl = "identity/national-code/check";
    private const string ServiceName = "Shahkar";


    public Endpoint(IHttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    public override void Configure()
    {
        Post(ApiUrl);
        Permissions("shahkar_identity_national-code_v2");
        Version(1);
        Group<ShahkarGroup>();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var response = await _httpClient.PostRequestAsync<Request, Response>(ServiceName, ApiUrl, req);
        await SendOkAsync(response, ct);
    }
}

internal sealed record Request
{
    public string NationalCode { get; init; } = default!;

    public string PhoneNumber { get; init; } = default!;
}

internal sealed record Response
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;
}

internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "Getting matching and mobile number inquiry by national code.";
        Description = "Getting matching mobile number inquiry by national code, passport number, amish number, refuge number, identity number, national identifier, comprehensive passport number.";
        Response<Response?>(200, "Inquiry response was successfully returned");
    }
}