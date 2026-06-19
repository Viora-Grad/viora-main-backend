using Viora.Application.Abstractions.Media;

namespace Viora.Application.Organizations.GetApplicationDetails;

public record ApplicationDetailsResponse(
    Guid Id,
    Guid OwnerId,
    string OwnerName,
    string Name,
    string About,
    string Letter,
    string ServiceDescription,
    IEnumerable<string> ServicesProvided,
    DateTime SubmittedOnUtc,
    string Status,
    string ReferralSource,
    Guid? RejectedById,
    string? RejectedByName,
    DateTime ExpiryDateUtc,
    string BillingEmail,
    string SupportEmail,
    LegalPaper? ArticleOfAssociation,
    LegalPaper? CommercialRegistration,
    LegalPaper? RegisteredAddressProof,
    LegalPaper? TaxCard);

public sealed record LegalPaper(
    Guid Id,
    string Name,
    MediaResponse Media,
    Approval? ApprovedBy,
    DateTime SubmittedOnUtc,
    DateTime ExpiryDateUtc);

public sealed record Approval(Guid AdminId, string Name);