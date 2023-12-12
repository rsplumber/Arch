using Arch.Core.ServiceConfigs.EndpointDefinitions.Exceptions;
using Arch.Data.Abstractions.EndpointDefinitions;
using Microsoft.EntityFrameworkCore;

namespace Arch.Data.EF.EndpointDefinitions;

internal sealed class EndpointDefinitionQuery : IEndpointDefinitionQuery
{
    private readonly AppDbContext _dbContext;

    public EndpointDefinitionQuery(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<EndpointDefinitionQueryResponse> QueryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var endpointDefinition = await _dbContext.EndpointDefinitions
            .Select(definition => new EndpointDefinitionQueryResponse
            {
                Id = definition.Id,
                Method = definition.Method.ToString(),
                Pattern = definition.Pattern,
                Endpoint = definition.Endpoint,
                MapTo = definition.MapTo,
                Meta = definition.Meta.ToDictionary(a => a.Key, a => string.Join(";", a.Value!))
            })
            .FirstOrDefaultAsync(definition => definition.Id == id, cancellationToken);

        if (endpointDefinition is null) throw new EndpointDefinitionNotFoundException();

        return endpointDefinition;
    }
}