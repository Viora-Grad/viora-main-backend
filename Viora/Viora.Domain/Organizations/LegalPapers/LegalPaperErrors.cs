using Viora.Domain.Abstractions;

namespace Viora.Domain.Organizations.LegalPapers;

public static class LegalPaperErrors
{
    public static readonly Error AlreadyExpired = new("LegalPapers.AlreadyExpired", "The legal papers already marked as expired", ErrorCategory.Conflict);
    public static readonly Error PaperExistsAndUpdated = new("LegalPapers.PaperExistsAndUpdated", "Paper already exists and an action has been taken on it", ErrorCategory.Conflict);
    public static readonly Error PaperStatusNotUnderReview
        = new("LegalPapers.PaperStatusNotUnderReview", "Paper must be under review to change its status to either accept or deny", ErrorCategory.Conflict);
    public static readonly Error NotFound = new("LegalPapers.NotFound", "The requested legal paper was not found", ErrorCategory.NotFound);
    public static readonly Error FileAccessDenied = new("LegalPapers.FileAccessDenied", "You are not allowed to access this legal paper", ErrorCategory.Forbidden);
    public static readonly Error FileMissing = new("LegalPapers.FileMissing", "The legal paper's file could not be located", ErrorCategory.NotFound);

}
