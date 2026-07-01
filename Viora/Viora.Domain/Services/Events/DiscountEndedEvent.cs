using Viora.Domain.Abstractions;

namespace Viora.Domain.Services.Events;

public sealed record DiscountEndedEvent(Guid ServiceId) : IDomainEvent;
