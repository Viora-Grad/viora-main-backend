namespace Viora.Api.Controllers.Oganizations;

public record SuspendOrganizationRequest(
    string Reason,
    string Notes);