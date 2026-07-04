using Viora.Domain.Abstractions;

namespace Viora.Application.Abstractions.Scheduling;

public interface IDomainEventScheduler
{
    Task<Guid> ScheduleAsync<TEvent>(TEvent @event, DateTimeOffset scheduledFor, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
    Task CancelAsync(Guid scheduledEventId, CancellationToken cancellationToken = default);
}