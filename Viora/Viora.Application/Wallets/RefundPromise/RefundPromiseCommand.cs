using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Wallets.RefundPromise;

// Refunds a pending promise back to the customer wallet (expiry / cancel / no-show). Idempotent.
public sealed record RefundPromiseCommand(Guid PromiseId) : ICommand;
