using Viora.Domain.Abstractions;
using Viora.Domain.Wallets.Internals;
using Viora.Domain.WalletTransactions;

namespace Viora.Domain.Wallets;

public sealed class Wallet : Entity
{
    public Guid OwnerId => UserId ?? BranchId ?? throw new InvalidOperationException("Both UserId and BranchId cannot be null.");
    public WalletType Type => UserId == null ? WalletType.Branch : WalletType.Customer;

    public Guid? UserId { get; private set; }
    public Guid? BranchId { get; private set; }

    public DateTime OpenedAtUtc { get; private set; }

    private readonly List<WalletTransaction> _transactions = [];
    public IReadOnlyCollection<WalletTransaction> Transactions => _transactions.AsReadOnly();

    public static Wallet Create(Guid ownerId, WalletType type, DateTime currentDateTime)
    {
        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            OpenedAtUtc = currentDateTime
        };

        if (type == WalletType.Branch)
            wallet.BranchId = ownerId;
        else
            wallet.UserId = ownerId;

        return wallet;
    }
}
