namespace Viora.Domain.Wallets;

public interface IWalletSettings
{
    /// <summary>
    /// Minutes added to an appointment's reservation date to compute a payment promise's expiry.
    /// If the customer never checks in, the held funds are refunded after this window.
    /// </summary>
    int PromiseGraceMinutes { get; set; }
}
