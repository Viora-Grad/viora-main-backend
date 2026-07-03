using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Wallets.CompletePromise;

// Settles a pending promise: credits the branch wallet and cancels the scheduled expiry. Idempotent.
public sealed record CompletePromiseCommand(Guid PromiseId) : ICommand;
