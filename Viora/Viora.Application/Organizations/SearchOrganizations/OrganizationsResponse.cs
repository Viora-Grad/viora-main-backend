using Viora.Application.Abstractions.Media;

namespace Viora.Application.Organizations.SearchOrganizations;

// TODO add simple branches hierarchy to avoid N + 1 calls
public record OrganizationsResponse(
    Guid Id,
    MediaResponse Logo,
    string Name,
    string Country,
    string ServiceDescription,
    IEnumerable<string> ServicesProvided,
    int RatingsCount,
    double RatingOutOfTen);