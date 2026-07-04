using Viora.Domain.Abstractions;

namespace Viora.Domain.Wallets;

public static class WalletErrors
{
    public static readonly Error InsufficientFunds =
        new("Wallets.InsufficientFunds", "Insufficient wallet balance for this operation", ErrorCategory.Validation);

    public static readonly Error WalletNotFound =
        new("Wallets.NotFound", "The wallet for customer, please open one and try again, was not found", ErrorCategory.NotFound);

    public static readonly Error WalletBranchNotFound =
        new("Wallets.WalletBranchNotFoind", "The wallet for Branch was not found or has not been opened, Contact branch admin", ErrorCategory.NotFound);


    public static readonly Error WalletAlreadyExists =
        new("Wallets.AlreadyExists", "A wallet already exists for this owner", ErrorCategory.Conflict);

    public static readonly Error CheckoutNotAllowed =
        new("Wallets.CheckoutNotAllowed", "Only branch wallets can check out", ErrorCategory.Validation);

    public static readonly Error CurrencyMismatch =
        new("Wallets.CurrencyMismatch", "The transaction currency does not match the wallet currency", ErrorCategory.Validation);
}
