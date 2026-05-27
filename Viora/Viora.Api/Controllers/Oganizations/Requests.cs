using Viora.Domain.Organizations.Shared.Enums;

namespace Viora.Api.Controllers.Oganizations;

// TODO adjust these fields to be extracted from HTTP context after auth is done and mark them as NotMapped
public record RequestOnboardRequest(
    Guid OwnerId,
    Guid CountryId,
    string ProposedName,
    string About,
    string ServiceDescription,
    string Letter,
    ICollection<ServiceType> ServiceTypes,
    ReferralSource ReferralSource,
    string BillingEmail,
    string SupportEmail);

public record SuspendOrganizationRequest(
    Guid? SuspendedById,
    string Reason,
    string Notes);

public record UpdateLogoRequest(Guid MediaId);
