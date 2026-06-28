using Viora.Domain.Organizations.Shared.Enums;

namespace Viora.Api.Controllers.Applications;

public record RequestOnboardRequest(
    Guid CountryId,
    string ProposedName,
    string About,
    string ServiceDescription,
    string Letter,
    ICollection<string> ServiceTypes,
    ReferralSource ReferralSource,
    string BillingEmail,
    string SupportEmail);