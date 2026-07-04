using Viora.Domain.Wallets.Internals;

namespace Viora.Domain.Wallets;

public interface IWalletRepository
{
    void Add(Wallet wallet);

    Task<Wallet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Wallet?> GetByOwnerAsync(Guid ownerId, WalletType type, CancellationToken cancellationToken = default);

    Task<bool> ExistsForOwnerAsync(Guid ownerId, WalletType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads a wallet with a pessimistic row lock (UPDLOCK, ROWLOCK). MUST be called inside an open
    /// transaction that stays open through SaveChanges; the lock is held until commit. Serializes
    /// concurrent mutations of the same wallet (prevents double-spend / lost updates).
    /// </summary>
    Task<Wallet?> GetForUpdateAsync(Guid walletId, CancellationToken cancellationToken = default);
}
