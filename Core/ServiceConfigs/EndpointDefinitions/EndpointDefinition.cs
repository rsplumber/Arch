using Arch.Core.ServiceConfigs.EndpointDefinitions.Events;
using Arch.Core.ServiceConfigs.Exceptions;

namespace Arch.Core.ServiceConfigs.EndpointDefinitions;

public sealed class EndpointDefinition : BaseEntity
{
    private const string DisableKey = "disabled";
    private const string LoggingMetaKey = "logging";

    public Guid Id { get; set; }

    public string Pattern { get; set; } = default!;

    public string MapTo { get; set; } = default!;

    public string Endpoint { get; set; } = default!;

    public HttpMethod Method { get; set; } = default!;

    public Dictionary<string, string> Meta { get; set; } = [];

    public ServiceConfig ServiceConfig { get; set; } = default!;

    public void UpdateMeta(Dictionary<string, string> meta)
    {
        if (ServiceConfig.Primary)
        {
            throw new PrimaryServiceModificationException();
        }

        Meta = meta;
        AddDomainEvent(new EndpointDefinitionChangedEvent(Id));
    }

    public void AddMeta(string key, string value)
    {
        Meta.Remove(key);
        Meta.Add(key, value);
        AddDomainEvent(new EndpointDefinitionChangedEvent(Id));
    }

    public void RemoveMeta(string key)
    {
        Meta.Remove(key);
        AddDomainEvent(new EndpointDefinitionChangedEvent(Id));
    }

    public bool HasMeta(string key) => Meta.ContainsKey(key);

    public void Enable() => RemoveMeta(DisableKey);

    public void Disable() => AddMeta(DisableKey, "true");

    public bool IsDisabled() => Meta.ContainsKey(DisableKey);

    public LoggingOptions Logging
    {
        get
        {
            Meta.TryGetValue(LoggingMetaKey, out var value);
            return new(value);
        }
    }

    public sealed record LoggingOptions
    {
        private const string LoggingJustErrorsMetaValue = "error";

        public LoggingOptions(string? loggingMeta)
        {
            if (loggingMeta is null)
            {
                Enabled = false;
                return;
            }

            Enabled = true;
            JustError = loggingMeta == LoggingJustErrorsMetaValue;
        }

        public bool Enabled { get; private set; }

        public bool JustError { get; private set; }
    }
}