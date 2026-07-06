using Viora.Domain.Abstractions;

namespace Viora.Domain.Marketing;

public static class MarketingErrors
{
    public static readonly Error SessionNotFound =
        new("Marketing.SessionNotFound", "The marketing chat session was not found", ErrorCategory.NotFound);

    public static readonly Error NotOwner =
        new("Marketing.NotOwner", "This marketing chat session does not belong to your organization", ErrorCategory.Forbidden);

    public static readonly Error OrganizationMissing =
        new("Marketing.OrganizationMissing", "No organization is associated with the current user", ErrorCategory.Forbidden);

    public static readonly Error CredentialNotFound =
        new("Marketing.CredentialNotFound", "No active Facebook Page credential is configured for this organization", ErrorCategory.Validation);

    public static readonly Error InvalidStatusForFinalize =
        new("Marketing.InvalidStatusForFinalize", "Only a draft chat session can be finalized into a post", ErrorCategory.Conflict);

    public static readonly Error InvalidStatusForPublish =
        new("Marketing.InvalidStatusForPublish", "Only an archived post can be published", ErrorCategory.Conflict);

    public static readonly Error ManusFailed =
        new("Marketing.ManusFailed", "The marketing content service failed to generate a response", ErrorCategory.BadGateway);

    public static readonly Error ContentGenerationFailed =
        new("Marketing.ContentGenerationFailed", "The AI could not produce a valid post payload", ErrorCategory.BadGateway);

    public static readonly Error PayloadInvalid =
        new("Marketing.PayloadInvalid", "The generated post payload was invalid", ErrorCategory.Validation);

    public static readonly Error MetaGraphFailed =
        new("Marketing.MetaGraphFailed", "The Facebook Graph API request failed", ErrorCategory.BadGateway);

    public static readonly Error NoDraftContent =
        new("Marketing.NoDraftContent", "There is no marketing content to finalize yet; generate content first", ErrorCategory.Validation);

    public static readonly Error ImageNotFound =
        new("Marketing.ImageNotFound", "No generated image is available for this chat", ErrorCategory.NotFound);
}
