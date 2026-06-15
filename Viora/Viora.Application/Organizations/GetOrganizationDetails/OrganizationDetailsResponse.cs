namespace Viora.Application.Organizations.GetOrganizationDetails;

public record OrganizationDetailsResponse(
    Guid Id,
    string Name,
    string About,
    string Country,
    string CountryCode,
    IEnumerable<string> ServicesProvided,
    string ServiceDescription,
    string ContactEmail,
    DateTime JoinedOnUtc,
    IEnumerable<MinimalBranch> Branches);

public record MinimalBranch(
    Guid Id,
    Guid? ImageId,
    string Address,
    DateTime OpenedSinceUtc);