using Viora.Domain.WalletTransactions.Internals;

namespace Viora.Domain.WalletTransactions;

public interface IWalletTransactionsRepository
{
    public Task<IReadOnlyCollection<WalletTransaction>> GetByWalletIdAsync(Guid walletId, int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>Idempotency pre-check: has a transaction with this (type, purpose, reference) already been written?</summary>
    public Task<bool> ExistsAsync(TransactionType type, Purpose purpose, string referenceId, CancellationToken cancellationToken);
}
