using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Wallets.ProvisionRecharge;

// Credits a user's wallet after a confirmed recharge payment. Dispatched by the recharge webhook.
// Idempotent via the gateway TransactionId (ReferenceId).
public sealed record ProvisionRechargeCommand(
    Guid UserId,
    decimal Amount,
    string Currency,
    string ReferenceId) : ICommand;
