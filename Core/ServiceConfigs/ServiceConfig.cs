using Core.EndpointDefinitions;
using Core.EndpointDefinitions.Events;
using Core.Metas;
using Core.ServiceConfigs.Events;

namespace Core.ServiceConfigs;

public sealed class ServiceConfig : BaseEntity
{
    private const string BaseUrlKey = "base_url";
    private const string IgnoreDispatchKey = "ignore_dispatch";


    protected ServiceConfig()
    {
    }

    private ServiceConfig(string name, string baseUrl)
    {
        Name = name;
        BaseUrl = baseUrl;
        AddDomainEvent(new ServiceConfigCreatedEvent(Id));
    }


    public static ServiceConfig Create(string name, string baseUrl, Dictionary<string, string>? meta = null)
    {
        var config = new ServiceConfig(name, baseUrl);
        config.Meta = meta is null ? new List<Meta>() : config.CalculateMeta(meta);
        return config;
    }

    public static ServiceConfig CreatePrimary(string name, string baseUrl, Dictionary<string, string>? meta = null)
    {
        var config = Create(name, baseUrl, meta);
        config.Primary = true;
        return config;
    }

    public Guid Id { get; set; }

    public string Name { get; private set; } = default!;

    public bool Primary { get; private set; }

    public string BaseUrl { get; private set; } = default!;

    public List<EndpointDefinition> EndpointDefinitions { get; set; } = new();

    public List<Meta> Meta { get; private set; } = new();

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    public void Update(string name, string baseUrl, Dictionary<string, string>? meta = null)
    {
        Name = name;
        BaseUrl = baseUrl;
        Meta = meta is null ? new List<Meta>() : CalculateMeta(meta);
        AddDomainEvent(new ServiceConfigChangedEvent(Id));
    }


    public void Add(Meta meta)
    {
        Meta.Add(meta);
        AddDomainEvent(new ServiceConfigChangedEvent(Id));
    }

    public void Remove(Meta meta)
    {
        Meta.Remove(meta);
        AddDomainEvent(new ServiceConfigChangedEvent(Id));
    }

    public void Add(EndpointDefinition definition)
    {
        if (HasEndpoint(definition.Pattern, definition.Method)) return;
        EndpointDefinitions.Add(definition);
        AddDomainEvent(new EndpointDefinitionCreatedEvent(definition.Id, Id));
    }

    public void Remove(EndpointDefinition definition)
    {
        EndpointDefinitions.Remove(definition);
        AddDomainEvent(new EndpointDefinitionRemovedEvent(definition.Id, Id));
    }

    private List<Meta> CalculateMeta(Dictionary<string, string> meta)
    {
        var finalMeta = new List<Meta>();
        AddAndSanitizeBaseUrlForMeta();
        foreach (var (key, value) in meta)
        {
            finalMeta.Add(new Meta
            {
                Key = key,
                Value = value
            });
        }

        return finalMeta;

        void AddAndSanitizeBaseUrlForMeta()
        {
            finalMeta.Add(new Meta
            {
                Key = BaseUrlKey,
                Value = BaseUrl
            });
            meta.Remove(BaseUrlKey);
        }
    }

    public void SetIgnoreDispatch() => Meta.Add(new()
    {
        Key = IgnoreDispatchKey,
        Value = "true"
    });

    public bool IgnoreDispatch() => Meta.Any(meta => meta.Key == IgnoreDispatchKey);

    public bool HasEndpoint(string pattern, HttpMethod method) => EndpointDefinitions.Any(definition => definition.Pattern == pattern
                                                                                                        && definition.Method == method);
}