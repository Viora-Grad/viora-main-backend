namespace Viora.Api.Controllers.Oganizations;

public record SuspendOrganizationRequest(
    string Reason,
    string Notes);

public record UpdateOrganizationProfileRequest(
    string SubDomain,
    string SupportEmail,
    string BillingEmail,
    string ServiceDescription,
    IReadOnlyList<string> ServicesProvided,
    string About);