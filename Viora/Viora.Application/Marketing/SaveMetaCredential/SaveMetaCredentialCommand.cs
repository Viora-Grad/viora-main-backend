using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Marketing.SaveMetaCredential;

// Saves/updates the caller's organization Facebook Page credential. Token is encrypted before storage.
public sealed record SaveMetaCredentialCommand(string PageId, string AccessToken) : ICommand;
