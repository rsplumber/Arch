using FastEndpoints;
using Microsoft.Extensions.Options;

namespace Arch.Endpoints.SabtAhval.Image;

internal sealed class Endpoint : Endpoint<Request, string>
{
    private readonly IHttpClient _httpClient;
    private readonly SabtAhvalOptions _sabtAhvalOptions;
    private const string ApiUrl = "inquiry/national-card/image";

    public Endpoint(IOptions<SabtAhvalOptions> sabtAhvalOptions, IHttpClient httpClient)
    {
        _httpClient = httpClient;
        _sabtAhvalOptions = sabtAhvalOptions.Value ?? throw new ArgumentNullException(nameof(sabtAhvalOptions), "enter sabtAhval options");
    }

    public override void Configure()
    {
        Post(ApiUrl);
        Permissions("sabtahval_inquiry_national-card_image_v2");
        Version(2);
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var response = await _httpClient.PostRequestAsync<Request, string>(_sabtAhvalOptions.BaseUrl + ApiUrl, req);
    }
}

internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "Getting Image from SabtAhval by CardSerial";
        Description = "Getting Image from SabtAhval by CardSerial.";
        Response<string>(200, "Inquiry response was successfully returned");
        Response(404, "Identity not found");
        Response(500, "Internal server error");
    }
}

internal sealed record Request
{
    public string NationalCode { get; init; } = default!;

    public string CardSerial { get; init; } = default!;
}