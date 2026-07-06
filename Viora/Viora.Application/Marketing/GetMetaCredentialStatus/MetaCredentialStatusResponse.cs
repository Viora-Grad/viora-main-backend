namespace Viora.Application.Marketing.GetMetaCredentialStatus;

// Connected=true means a Facebook Page credential is saved for the organization.
// PageId is echoed back (non-secret) so the UI can show which Page is linked; null when not connected.
public sealed record MetaCredentialStatusResponse(bool Connected, string? PageId);
