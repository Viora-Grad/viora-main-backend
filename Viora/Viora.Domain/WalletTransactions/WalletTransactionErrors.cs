using Viora.Domain.Abstractions;

namespace Viora.Domain.WalletTransactions;

public static class WalletTransactionErrors
{
    public static readonly Error AmountLessThanZero = new("WalletTransactions.AmountLessThanZero", "Can not register a negative amount", ErrorCategory.Validation);
}
