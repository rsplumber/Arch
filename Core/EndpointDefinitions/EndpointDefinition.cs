﻿using System.Globalization;
using Core.EndpointDefinitions.Events;
using Core.Metas;
using Core.ServiceConfigs;
using Core.ServiceConfigs.Exceptions;

namespace Core.EndpointDefinitions;

public sealed class EndpointDefinition : BaseEntity
{
    private const string DisableKey = "disable";

    public Guid Id { get; set; }

    public string Pattern { get; set; } = default!;

    public string MapTo { get; set; } = default!;

    public string Endpoint { get; set; } = default!;

    public HttpMethod Method { get; set; } = default!;

    public List<Meta> Meta { get; set; } = new();

    public ServiceConfig ServiceConfig { get; set; } = default!;

    public void UpdateMeta(Dictionary<string, string> meta)
    {
        if (ServiceConfig.Primary)
        {
            throw new PrimaryServiceModificationException();
        }

        Meta.Clear();
        foreach (var (key, value) in meta)
        {
            Meta.Add(new Meta
            {
                Key = key,
                Value = value
            });
        }

        AddDomainEvent(new EndpointDefinitionChangedEvent(Id));
    }

    public bool IsDisabled() => Meta.Any(meta => meta.Key == DisableKey);

    public string GenerateRequestPath(object[] pathParameters) => string.Format(CultureInfo.CurrentCulture, MapTo, pathParameters);
}