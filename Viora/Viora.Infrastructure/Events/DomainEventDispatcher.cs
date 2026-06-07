using MediatR;
using Viora.Domain.Abstractions;
using Viora.Domain.Events;

namespace Viora.Infrastructure.Events;

public class DomainEventDispatcher(IPublisher publisher) : IDomainEventDispatcher
{
    private readonly IPublisher _publisher = publisher;

    public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in events)
        {
            await _publisher.Publish(
                domainEvent,
                cancellationToken);
        }
    }
}

