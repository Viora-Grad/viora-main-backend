namespace Viora.Domain.WalletTransactions;

internal interface IWalletTransactionsRepository
{
    public Task<IReadOnlyCollection<WalletTransaction>> GetByWalletIdAsync(Guid walletId, CancellationToken cancellationToken);
}
