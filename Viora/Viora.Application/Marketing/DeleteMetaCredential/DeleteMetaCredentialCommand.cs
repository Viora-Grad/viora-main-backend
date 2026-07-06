using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Marketing.DeleteMetaCredential;

// Removes the caller's organization Facebook Page credential from the database. Idempotent.
public sealed record DeleteMetaCredentialCommand : ICommand;
