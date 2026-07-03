namespace Viora.Domain.WalletPromises;

public interface IWalletPromiseRepository
{
    void Add(WalletPromise promise);

    Task<WalletPromise?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Finds a promise by its source (hold) transaction id — the value stored on Appointment.PaymentId.</summary>
    Task<WalletPromise?> GetBySourceTransactionIdAsync(Guid sourceTransactionId, CancellationToken cancellationToken = default);
}
