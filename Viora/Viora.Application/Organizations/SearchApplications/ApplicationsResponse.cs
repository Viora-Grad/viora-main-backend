namespace Viora.Application.Organizations.SearchApplications;

public record ApplicationsResponse(
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
    string SupportEmail);