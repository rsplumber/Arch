using Arch.Data.Abstractions.ServiceConfigs;
using FastEndpoints;

namespace Arch.Endpoints.ServiceConfigs.List;

internal sealed class Endpoint : Endpoint<Request, List<ServiceConfigsQueryResponse>>
{
    private readonly IServiceConfigsQuery _query;

    public Endpoint(IServiceConfigsQuery query)
    {
        _query = query;
    }

    public override void Configure()
    {
        Get("service-configs");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        var response = await _query.QueryAsync(request.Name, ct);
        await HttpContext.Response.SendOkAsync(response, null, ct);
    }
}

internal sealed record Request
{
    public string? Name { get; init; } = default;
}