using Arch.Core.ServiceConfigs.EndpointDefinitions.Events;
using Arch.Core.ServiceConfigs.Events;
using EndpointDefinition = Arch.Core.ServiceConfigs.EndpointDefinitions.EndpointDefinition;

namespace Arch.Core.ServiceConfigs;

public class ServiceConfig : BaseEntity
{
    private const string IgnoreDispatchKey = "ignore_dispatch";

    protected ServiceConfig()
    {
    }

    private ServiceConfig(string name, string baseUrl)
    {
        Name = name;
        BaseUrls.Add(baseUrl);
        AddDomainEvent(new ServiceConfigCreatedEvent(Id));
    }


    public static ServiceConfig Create(string name, string baseUrl, Dictionary<string, string>? meta = null)
    {
        return new ServiceConfig(name, baseUrl)
        {
            Meta = meta ?? []
        };
    }

    public static ServiceConfig CreatePrimary(string name, string baseUrl, Dictionary<string, string>? meta = null)
    {
        return new ServiceConfig(name, baseUrl)
        {
            Meta = meta ?? [],
            Primary = true
        };
    }

    public Guid Id { get; set; }

    public string Name { get; private set; } = default!;

    public bool Primary { get; private set; }

    public List<string> BaseUrls { get; private set; } = [];

    public List<EndpointDefinition> EndpointDefinitions { get; set; } = [];

    public Dictionary<string, string> Meta { get; private set; } = [];

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    public void Update(string name, string baseUrl, Dictionary<string, string>? meta = null)
    {
        Name = name;
        if (!BaseUrls.Any(url => url.Equals(baseUrl)))
        {
            BaseUrls.Add(baseUrl);
        }

        Meta = meta ?? [];
        AddDomainEvent(new ServiceConfigChangedEvent(Id));
    }


    public void AddMeta(string key, string value)
    {
        Meta.Remove(key);
        Meta.Add(key, value);
        AddDomainEvent(new ServiceConfigChangedEvent(Id));
    }

    public void RemoveMeta(string key)
    {
        Meta.Remove(key);
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


    public void SetIgnoreDispatch()
    {
        Meta.Remove(IgnoreDispatchKey);
        Meta.Add(IgnoreDispatchKey, "true");
    }

    public bool IgnoreDispatch() => Meta.ContainsKey(IgnoreDispatchKey);

    public bool HasEndpoint(string pattern, HttpMethod method) => EndpointDefinitions.Any(definition => definition.Pattern == pattern
                                                                                                        && definition.Method == method);
}