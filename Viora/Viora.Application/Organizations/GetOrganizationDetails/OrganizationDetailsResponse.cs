namespace Viora.Application.Organizations.GetOrganizationDetails;

public record OrganizationDetailsResponse(
    Guid Id,
    string Name,
    string Country,
    string CountryCode,
    string ServiceType,
    string ServiceDescription,
    string ContactEmail,
    DateOnly JoinedOnUtc,
    IEnumerable<Branch> Branches
    );


// TODO update when the branch module is completed this is just a placeholder for future reminder on the composition.
public record Branch(
    Guid Id,
    string Location,
    string Address,
    string ContactNumber
    );