using Arch.Core;
using DotNetCore.CAP;

namespace Arch.Data.EF;

internal static class CapExtension
{
    public static async Task DispatchDomainEventsAsync(this ICapPublisher eventBus, AppDbContext ctx)
    {
        var domainEntities = ctx.ChangeTracker
            .Entries<BaseEntity>()
            .Where(x => x.Entity.DomainEvents is not null && x.Entity.DomainEvents.Any())
            .ToList();

        var domainEvents = domainEntities
            .Where(x => x.Entity.DomainEvents is not null)
            .SelectMany(x => x.Entity.DomainEvents!)
            .ToList();

        foreach (var domainEvent in domainEvents)
        {
            var eventAttribute = (DomainEvent)domainEvent;
            await eventBus.PublishAsync(eventAttribute.Name, domainEvent);
        }

        domainEntities.ForEach(entityEntry => entityEntry.Entity.ClearDomainEvent());
    }
}