namespace Viora.Application.Organizations.SearchOrganizations;

public record OrganizationsResponse(
    Guid Id,
    Guid? LogoId,
    string Name,
    string Country,
    string ServiceDescription,
    IEnumerable<string> ServicesProvided,
    int RatingsCount,
    double RatingOutOfTen);
