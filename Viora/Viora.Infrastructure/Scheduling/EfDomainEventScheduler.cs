using System.Text.Json;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Scheduling;
using Viora.Domain.Abstractions;
using Viora.Domain.Scheduling;

namespace Viora.Infrastructure.Scheduling;

internal class EfDomainEventScheduler(ApplicationDbContext appDbContext, IDateTimeProvider dateTime) : IDomainEventScheduler
{
    public Task ScheduleAsync<TEvent>(
    TEvent @event, DateTimeOffset scheduledFor, CancellationToken cancellationToken = default)
    where TEvent : IDomainEvent
    {
        var record = ScheduledDomainEvent.Create(
            eventType: typeof(TEvent).AssemblyQualifiedName!,
            payload: JsonSerializer.Serialize(@event, @event.GetType()),
            scheduledFor: scheduledFor);

        appDbContext.Set<ScheduledDomainEvent>().Add(record.Value);
        return Task.CompletedTask;
    }

    public async Task CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await appDbContext.Set<ScheduledDomainEvent>().FindAsync([id], cancellationToken);
        record?.Cancel(dateTime.UtcNow);
    }
}
