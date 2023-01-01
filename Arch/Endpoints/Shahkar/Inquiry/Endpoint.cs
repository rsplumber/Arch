using FastEndpoints;
using Microsoft.Extensions.Options;

namespace Arch.Endpoints.Shahkar.Inquiry;

internal sealed class Endpoint : Endpoint<Request, Response?>
{
    private readonly IHttpClient _httpClient;
    private readonly ShahkarOptions _shahkarOptions;
    private const string ApiUrl = "identity/national-code/check";


    public Endpoint(IHttpClient httpClient, IOptions<ShahkarOptions> shahkarOptions)
    {
        _httpClient = httpClient;
        _shahkarOptions = shahkarOptions.Value ?? throw new ArgumentNullException(nameof(shahkarOptions), "enter Shahkar options");
    }


    public override void Configure()
    {
        Post(ApiUrl);
        Permissions("shahkar_identity_national-code_v2");
        Version(1);
        Tags("Shahkar");
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var response = await _httpClient.PostRequestAsync<Request, Response>(_shahkarOptions.BaseUrl + ApiUrl, req);
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