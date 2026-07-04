using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Wallets.Checkout;

// Branch wallet payout via a Kashier single bank transfer, then debits the wallet. Bank transfers only.
public sealed record CheckoutCommand(
    Guid BranchId,
    decimal Amount,
    string Currency,
    string RecipientName,
    string RecipientBank,
    string RecipientNumber) : ICommand<CheckoutResponse>;

public sealed record CheckoutResponse(string TransferReference);
