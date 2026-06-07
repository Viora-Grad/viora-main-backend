using Viora.Domain.Abstractions;

namespace Viora.Application.Abstractions.Events;

public interface IDomainEventPublisher
{
    Task PublishAsync(IDomainEvent @event, CancellationToken ct = default);
}