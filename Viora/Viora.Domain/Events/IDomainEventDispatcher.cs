using Viora.Domain.Abstractions;

namespace Viora.Domain.Events;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(
       IEnumerable<IDomainEvent> events,
       CancellationToken cancellationToken = default);
}
