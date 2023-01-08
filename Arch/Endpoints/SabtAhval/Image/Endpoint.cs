using FastEndpoints;

namespace Arch.Endpoints.SabtAhval.Image;

internal sealed class Endpoint : Endpoint<Request, string>
{
    private readonly IHttpClient _httpClient;
    private const string ApiUrl = "inquiry/national-card/image";
    private const string ServiceName = "SabtAhval";

    public Endpoint(IHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public override void Configure()
    {
        Post(ApiUrl);
        Permissions("sabtahval_inquiry_national-card_image_v2");
        Version(1);
        Group<SabtAhvalGroup>();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var response = await _httpClient.PostRequestAsync<Request, string>(ServiceName, ApiUrl, req);
    }
}

internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "Getting Image from SabtAhval by CardSerial";
        Description = "Getting Image from SabtAhval by CardSerial.";
        Response<string>(200, "Inquiry response was successfully returned");
    }
}

internal sealed record Request
{
    public string NationalCode { get; init; } = default!;

    public string CardSerial { get; init; } = default!;
}