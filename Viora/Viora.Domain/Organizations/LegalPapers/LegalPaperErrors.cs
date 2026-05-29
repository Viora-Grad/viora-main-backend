using Viora.Domain.Abstractions;

namespace Viora.Domain.Organizations.LegalPapers;

internal static class LegalPaperErrors
{
    public static readonly Error AlreadyExpired = new("LegalPapers.AlreadyExpired", "The legal papers already marked as expired", ErrorCategory.Conflict);
}
