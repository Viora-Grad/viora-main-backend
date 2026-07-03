using Viora.Domain.Abstractions;
using Viora.Domain.Shared;
using Viora.Domain.WalletTransactions.Internals;

namespace Viora.Domain.WalletTransactions;

public class WalletTransaction : Entity
{
    public Guid WalletId { get; private set; }
    public TransactionType Type { get; private set; }
    public Description Description { get; private set; } = default!;
    public Purpose Purpose { get; private set; }
    public Money Money { get; private set; } = default!;
    public Money RunningBalance { get; private set; } = default!;
    public DateTime CreatedAtUtc { get; private set; }
    public ExternalReferenceId ReferenceId { get; private set; } = default!;

    public Money EffectiveAmount => Type == TransactionType.Credit ? Money : new(-Money.Amount, Money.Currency);

    private WalletTransaction() { }

    public static Result<WalletTransaction> Create(
        Guid walletId,
        TransactionType type,
        string description,
        Purpose purpose,
        Money amount,
        Money currentBalance,
        DateTime currentDateTime,
        string referenceId)
    {
        if (amount.Amount <= 0)
            return Result.Failure<WalletTransaction>(WalletTransactionErrors.AmountLessThanZero);

        var transaction = new WalletTransaction()
        {
            Id = Guid.NewGuid(),
            WalletId = walletId,
            Type = type,
            Description = description,
            Purpose = purpose,
            Money = amount,
            CreatedAtUtc = currentDateTime,
            ReferenceId = referenceId
        };

        transaction.RunningBalance = currentBalance + transaction.EffectiveAmount;

        return Result.Success(transaction);
    }
}
