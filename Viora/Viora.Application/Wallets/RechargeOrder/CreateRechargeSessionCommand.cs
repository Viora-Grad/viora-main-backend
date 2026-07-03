using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Wallets.RechargeOrder;

// Starts a wallet top-up: creates a hosted payment session. No intent is persisted — only a successful
// payment (via the recharge webhook) credits the wallet.
public sealed record CreateRechargeSessionCommand(decimal Amount) : ICommand<CreateRechargeSessionResponse>;

public sealed record CreateRechargeSessionResponse(string PaymentUrl);
