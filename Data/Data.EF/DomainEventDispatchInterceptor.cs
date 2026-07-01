using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Arch.Data.EF;

/// <summary>
/// Publishes the domain events accumulated on tracked entities after they have been persisted.
/// Runs on <see cref="SavedChangesAsync"/> (post-save) so database-generated keys are populated
/// before the events are dispatched. Registered as a singleton to stay compatible with the pooled
/// <see cref="AppDbContext"/>; the scoped <see cref="ICapPublisher"/> is resolved per save through a
/// fresh scope.
/// </summary>
internal sealed class DomainEventDispatchInterceptor : SaveChangesInterceptor
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DomainEventDispatchInterceptor(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is AppDbContext appDbContext)
        {
            using var scope = _scopeFactory.CreateScope();
            var eventBus = scope.ServiceProvider.GetRequiredService<ICapPublisher>();
            await eventBus.DispatchDomainEventsAsync(appDbContext);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}
