namespace Viora.Application.Organizations.SearchApplications;

public record ApplicationsResponse(
    Guid Id,
    Guid OwnerId,
    string OwnerName,
    string Name,
    string Letter,
    string ServiceDescription,
    string ServiceType,
    DateTime SubmittedOnUtc,
    string Status,
    string ReferralSource,
    Guid? RejectedById,
    string? RejectedByName,
    DateTime ExpiryDateUtc,
    string BillingEmail,
    string SupportEmail);
