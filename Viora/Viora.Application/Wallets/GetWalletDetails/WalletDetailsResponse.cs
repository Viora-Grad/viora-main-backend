namespace Viora.Application.Wallets.GetWalletDetails;

public sealed record WalletDetailsResponse(
    Guid WalletId,
    string WalletType,
    decimal Balance,
    string Currency,
    IReadOnlyCollection<WalletTransactionResponse> Transactions);

public sealed record WalletTransactionResponse(
    Guid Id,
    string Type,
    string Purpose,
    decimal Amount,
    string Currency,
    decimal RunningBalance,
    string Description,
    string ReferenceId,
    DateTime CreatedAtUtc);
