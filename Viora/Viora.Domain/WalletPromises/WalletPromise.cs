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

    /// <summary>
    /// Creates a pending promise (escrow). The <paramref name="id"/> is supplied by the caller so it can
    /// be used up-front as the idempotency reference for the source/settlement/refund transactions and as
    /// the scheduled-expiry event payload. <paramref name="sourceTransactionId"/> is the hold debit already
    /// taken from the source wallet.
    /// </summary>
    public static Result<WalletPromise> Create(
        Guid id,
        Guid fromWalletId,
        Guid toWalletId,
        Money amount,
        DateTime expiresAtUtc,
        DateTime currentDateTime,
        Guid sourceTransactionId)
    {
        if (fromWalletId == toWalletId)
            return Result.Failure<WalletPromise>(WalletPromiseErrors.CannotTransferToSelf);

        if (amount.Amount <= 0)
            return Result.Failure<WalletPromise>(WalletPromiseErrors.AmountLessThanZero);

        if (expiresAtUtc <= currentDateTime)
            return Result.Failure<WalletPromise>(WalletPromiseErrors.InvalidExpirationTime);

        var promise = new WalletPromise
        {
            Id = id,
            FromWalletId = fromWalletId,
            ToWalletId = toWalletId,
            Money = amount,
            ExpiresAtUtc = expiresAtUtc,
            Status = PromiseStatus.Pending,
            SourceTransactionId = sourceTransactionId,
        };

        return Result.Success(promise);
    }

    /// <summary>Records the scheduled expiry event id so it can be cancelled when the promise settles.</summary>
    public void AttachScheduledEvent(Guid scheduledEventId) => ScheduledEventId = scheduledEventId;

    /// <summary>Pending -&gt; Completed. Idempotent: fails if already resolved.</summary>
    public Result Complete(Guid destinationTransactionId)
    {
        if (Status != PromiseStatus.Pending)
            return Result.Failure(WalletPromiseErrors.AlreadyResolved);

        Status = PromiseStatus.Completed;
        DestinationTransactionId = destinationTransactionId;
        return Result.Success();
    }

    /// <summary>Pending -&gt; Refunded. Idempotent: fails if already resolved.</summary>
    public Result Refund()
    {
        if (Status != PromiseStatus.Pending)
            return Result.Failure(WalletPromiseErrors.AlreadyResolved);

        Status = PromiseStatus.Refunded;
        return Result.Success();
    }

    /// <summary>Pending -&gt; Failed. Idempotent: fails if already resolved.</summary>
    public Result Fail()
    {
        if (Status != PromiseStatus.Pending)
            return Result.Failure(WalletPromiseErrors.AlreadyResolved);

        Status = PromiseStatus.Failed;
        return Result.Success();
    }
}
