using FastEndpoints;
using Microsoft.Extensions.Options;

namespace Arch.Endpoints.SabtAhval.Inquiry;

internal sealed class Endpoint : Endpoint<Request, Response>
{
    private readonly SabtAhvalOptions _sabtAhvalOptions;
    private const string ApiUrl = "inquiry";

    public Endpoint(IOptions<SabtAhvalOptions> sabtAhvalOptions)
    {
        _sabtAhvalOptions = sabtAhvalOptions.Value ?? throw new ArgumentNullException(nameof(sabtAhvalOptions), "enter sabtAhval options");
    }

    public override void Configure()
    {
        Post(ApiUrl);
        Permissions("sabtahval_inquiry_v2");
        Version(2);
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
    }
}

internal sealed class Summary : Summary<Endpoint>
{
    public Summary()
    {
        Summary = "Getting Inquiry from SabtAhval by person infos";
        Description = "Getting Inquiry from SabtAhval by person infos.";
        Response<Response>(200, "Inquiry response was successfully returned");
    }
}

internal sealed record Request
{
    public string NationalCode { get; init; } = default!;

    public DateOnly BirthDate { get; init; } = default!;
}

internal sealed record Response
{
    public string BirthDate { get; init; } = default!;

    public int BookNumber { get; init; }

    public string Name { get; init; } = default!;

    public string LastName { get; init; } = default!;

    public string FatherName { get; init; } = default!;

    public string Gender { get; init; } = default!;

    public string NationalCode { get; init; } = default!;

    public string OfficeCode { get; init; } = default!;

    public string OfficeName { get; init; } = default!;

    public string ShenasnameNumber { get; init; } = default!;

    public string ShenasnameSeri { get; init; } = default!;

    public string ShenasnameSerial { get; init; } = default!;
}