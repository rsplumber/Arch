using Arch.Core.ServiceConfigs;
using Arch.Core.ServiceConfigs.EndpointDefinitions;
using Arch.Core.ServiceConfigs.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Admin.Services;

/// <summary>
/// Thin façade the Blazor circuit uses to read and mutate gateway configuration.
///
/// Blazor Server circuits are long-lived, so we never hold a scoped repository or the
/// pooled <c>AppDbContext</c> across awaits. Every operation opens its own short-lived
/// DI scope, resolves the same services the gateway's HTTP endpoints use, and disposes it.
/// This keeps behaviour identical to the FastEndpoints (enable/disable/add/remove) while
/// staying safe under concurrent circuit activity.
/// </summary>
public sealed class AdminConfigService
{
    /// <summary>Meta key the gateway uses to flag a disabled endpoint (see <c>EndpointDefinition</c>).</summary>
    private const string DisabledKey = "disabled";

    private readonly IServiceScopeFactory _scopeFactory;

    public AdminConfigService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    // ---- Reads ---------------------------------------------------------------

    public async Task<List<ServiceSummary>> GetServicesAsync(CancellationToken ct = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IServiceConfigRepository>();
        var configs = await repository.FindAsync(ct);
        return configs.Select(ToSummary)
            .OrderByDescending(s => s.Primary)
            .ThenBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<ServiceWithEndpoints?> GetServiceAsync(Guid id, CancellationToken ct = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IServiceConfigRepository>();
        var config = await repository.FindAsync(id, ct);
        if (config is null) return null;

        var endpoints = config.EndpointDefinitions
            .Select(ToSummary)
            .OrderBy(e => e.Endpoint, StringComparer.OrdinalIgnoreCase)
            .ThenBy(e => e.Method, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new ServiceWithEndpoints { Service = ToSummary(config), Endpoints = endpoints };
    }

    // ---- Service mutations ---------------------------------------------------

    public async Task CreateServiceAsync(string name, string baseUrl, Dictionary<string, string> meta, CancellationToken ct = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IServiceConfigService>();
        await service.CreateAsync(new CreateServiceConfigRequest
        {
            Name = name.Trim(),
            BaseUrl = baseUrl.Trim(),
            Meta = meta
        }, ct);
    }

    public async Task UpdateServiceAsync(Guid id, string name, string baseUrl, Dictionary<string, string> meta, CancellationToken ct = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IServiceConfigService>();
        await service.UpdateAsync(new UpdateServiceConfigRequest
        {
            Id = id,
            Name = name.Trim(),
            BaseUrl = baseUrl.Trim(),
            Meta = meta
        }, ct);
    }

    public async Task DeleteServiceAsync(Guid id, CancellationToken ct = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IServiceConfigService>();
        await service.DeleteAsync(id, ct);
    }

    /// <summary>
    /// Enables or disables a whole service. The gateway has no first-class "service off"
    /// switch, so disabling flags the service meta (for the UI) and disables every one of
    /// its endpoints — which is what actually stops traffic. Enabling reverses both.
    /// </summary>
    public async Task SetServiceDisabledAsync(Guid id, bool disabled, CancellationToken ct = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IServiceConfigRepository>();
        var config = await repository.FindAsync(id, ct);
        if (config is null) throw new InvalidOperationException("Service not found.");

        if (disabled) config.AddMeta(DisabledKey, "true");
        else config.RemoveMeta(DisabledKey);

        foreach (var definition in config.EndpointDefinitions)
        {
            if (disabled) definition.Disable();
            else definition.Enable();
        }

        await repository.UpdateAsync(config, ct);
    }

    // ---- Endpoint mutations --------------------------------------------------

    public async Task AddEndpointAsync(Guid serviceId, string endpoint, string mapTo, string method, Dictionary<string, string> meta, CancellationToken ct = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IEndpointDefinitionService>();
        await service.AddAsync(new AddEndpointDefinitionRequest
        {
            ServiceConfigId = serviceId,
            Endpoint = endpoint.Trim(),
            MapTo = mapTo.Trim(),
            Method = new HttpMethod(method.Trim().ToUpperInvariant()),
            Meta = meta
        }, ct);
    }

    public async Task RemoveEndpointAsync(Guid endpointId, CancellationToken ct = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IEndpointDefinitionService>();
        await service.RemoveAsync(endpointId, ct);
    }

    public async Task SetEndpointDisabledAsync(Guid endpointId, bool disabled, CancellationToken ct = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEndpointDefinitionRepository>();
        var definition = await repository.FindAsync(endpointId, ct);
        if (definition is null) throw new InvalidOperationException("Endpoint not found.");

        if (disabled) definition.Disable();
        else definition.Enable();

        await repository.UpdateAsync(definition, ct);
    }

    public async Task UpdateEndpointMetaAsync(Guid endpointId, Dictionary<string, string> meta, CancellationToken ct = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEndpointDefinitionRepository>();
        var definition = await repository.FindAsync(endpointId, ct);
        if (definition is null) throw new InvalidOperationException("Endpoint not found.");

        // UpdateMeta guards against modifying endpoints that belong to the primary service.
        definition.UpdateMeta(meta);
        await repository.UpdateAsync(definition, ct);
    }

    // ---- Mapping -------------------------------------------------------------

    private static ServiceSummary ToSummary(ServiceConfig config) => new()
    {
        Id = config.Id,
        Name = config.Name,
        Primary = config.Primary,
        Disabled = config.Meta.ContainsKey(DisabledKey),
        BaseUrls = config.BaseUrls.ToList(),
        Meta = new Dictionary<string, string>(config.Meta),
        EndpointCount = config.EndpointDefinitions.Count,
        DisabledEndpointCount = config.EndpointDefinitions.Count(d => d.IsDisabled()),
        CreatedAtUtc = config.CreatedAtUtc
    };

    private static EndpointSummary ToSummary(EndpointDefinition definition) => new()
    {
        Id = definition.Id,
        Method = definition.Method.ToString(),
        Endpoint = definition.Endpoint,
        Pattern = definition.Pattern,
        MapTo = definition.MapTo,
        Disabled = definition.IsDisabled(),
        Meta = new Dictionary<string, string>(definition.Meta)
    };
}
