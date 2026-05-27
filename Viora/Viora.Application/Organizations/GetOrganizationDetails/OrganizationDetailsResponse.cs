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
    IEnumerable<Branch> Branches
    );


// TODO update when the branch module is completed this is just a placeholder for future reminder on the composition.
public record Branch(
    Guid Id,
    string Location,
    string Address,
    string ContactNumber
    );