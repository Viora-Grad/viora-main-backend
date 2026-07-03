using Viora.Domain.Abstractions;

namespace Viora.Domain.WalletPromises.Events;

public sealed record PaymentPromisedEvent(Guid PaymentPromiseId, DateTime ExpiryDateUtc) : IDomainEvent;
