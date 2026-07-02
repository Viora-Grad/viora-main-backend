using Viora.Domain.Abstractions;
using Viora.Domain.Shared;
using Viora.Domain.WalletPromises.Internals;

namespace Viora.Domain.WalletPromises;

/// <summary>
/// Represents a promise to send money from a wallet to wallet instead of direct order
/// creates an event and on triggers delete the event and fills the payment of cancels it via the scheduled event
/// 
/// payment to appointment is made
/// it is held in the transient till the appointment is checked id
/// once the appointment is checked in the wallet action is made
/// another event is added on the transient with the event id to refund
/// if the payment is made delete that event from the db
/// </summary>
public sealed class WalletPromise : Entity
{
    public Guid ToWalletId { get; private set; }
    public Guid? ScheduledEventId { get; private set; }
    public Guid FromWalletId { get; private set; }
    public Money Money { get; private set; } = default!;
    public DateTime ExpiresAtUtc { get; private set; }
    public PromiseStatus Status { get; private set; } = PromiseStatus.Pending;

    public Guid SourceTransactionId { get; private set; }
    public Guid? DestinationTransactionId { get; private set; } = null;

    private WalletPromise() { }

    public static Result<WalletPromise> Create(
        Guid fromWalletId,
        Guid toWalletId,
        Money amount,
        DateTime expiresAtUtc,
        DateTime currentDateTime,
        Guid scheduledEventId)
    {
        if (fromWalletId == toWalletId)
            return Result.Failure<WalletPromise>(WalletPromiseErrors.CannotTransferToSelf);

        if (amount.Amount <= 0)
            return Result.Failure<WalletPromise>(WalletPromiseErrors.AmountLessThanZero);

        if (expiresAtUtc <= currentDateTime)
            return Result.Failure<WalletPromise>(WalletPromiseErrors.InvalidExpirationTime);

        var intent = new WalletPromise
        {
            Id = Guid.NewGuid(),
            FromWalletId = fromWalletId,
            ToWalletId = toWalletId,
            Money = amount,
            ScheduledEventId = scheduledEventId,
            ExpiresAtUtc = expiresAtUtc,
            Status = PromiseStatus.Pending
        };

        return Result.Success(intent);
    }
}
