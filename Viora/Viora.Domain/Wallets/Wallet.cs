using Viora.Domain.Abstractions;
using Viora.Domain.Shared;
using Viora.Domain.Wallets.Internals;
using Viora.Domain.WalletTransactions;
using Viora.Domain.WalletTransactions.Internals;

namespace Viora.Domain.Wallets;

public sealed class Wallet : Entity
{
    public Guid OwnerId => UserId ?? BranchId ?? throw new InvalidOperationException("Both UserId and BranchId cannot be null.");
    public WalletType Type => UserId == null ? WalletType.Branch : WalletType.Customer;

    public Guid? UserId { get; private set; }
    public Guid? BranchId { get; private set; }

    public Currency Currency { get; private set; } = default!;
    public Money Balance { get; private set; } = default!;

    public DateTime OpenedAtUtc { get; private set; }

    private readonly List<WalletTransaction> _transactions = [];
    public IReadOnlyCollection<WalletTransaction> Transactions => _transactions.AsReadOnly();

    private Wallet() { }

    public static Wallet Create(Guid ownerId, WalletType type, Currency currency, DateTime currentDateTime)
    {
        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            OpenedAtUtc = currentDateTime,
            Currency = currency,
            Balance = Money.Zero(currency),
        };

        if (type == WalletType.Branch)
            wallet.BranchId = ownerId;
        else
            wallet.UserId = ownerId;

        return wallet;
    }

    /// <summary>Appends a credit to the ledger and raises the balance. Returns the created transaction.</summary>
    public Result<WalletTransaction> Credit(Money amount, Purpose purpose, string description, string referenceId, DateTime currentDateTime)
        => Append(TransactionType.Credit, amount, purpose, description, referenceId, currentDateTime);

    /// <summary>Appends a debit to the ledger and lowers the balance. Fails on overdraft. Returns the created transaction.</summary>
    public Result<WalletTransaction> Debit(Money amount, Purpose purpose, string description, string referenceId, DateTime currentDateTime)
    {
        if (Balance.Amount < amount.Amount)
            return Result.Failure<WalletTransaction>(WalletErrors.InsufficientFunds);

        return Append(TransactionType.Debit, amount, purpose, description, referenceId, currentDateTime);
    }

    /// <summary>Capability guard: only branch wallets may check out (payout). Customer wallets cannot.</summary>
    public Result EnsureCanCheckout()
        => Type == WalletType.Customer
            ? Result.Failure(WalletErrors.CheckoutNotAllowed)
            : Result.Success();

    private Result<WalletTransaction> Append(TransactionType type, Money amount, Purpose purpose, string description, string referenceId, DateTime currentDateTime)
    {
        if (amount.Currency != Currency)
            return Result.Failure<WalletTransaction>(WalletErrors.CurrencyMismatch);

        var transactionResult = WalletTransaction.Create(Id, type, description, purpose, amount, Balance, currentDateTime, referenceId);
        if (transactionResult.IsFailure)
            return transactionResult;

        var transaction = transactionResult.Value;
        _transactions.Add(transaction);
        Balance = transaction.RunningBalance;

        return Result.Success(transaction);
    }
}
