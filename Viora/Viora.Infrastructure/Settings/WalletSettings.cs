using Viora.Domain.Wallets;

namespace Viora.Infrastructure.Settings;

public class WalletSettings : IWalletSettings
{
    // Default: one day after the reservation date.
    public int PromiseGraceMinutes { get; set; } = 1440;
}
