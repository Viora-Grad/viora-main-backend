using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;
using Viora.Domain.Organizations.OrganizationDetails.Internal;

namespace Viora.Application.Organizations.SearchOrganizations;

public record SearchOrganizationsQuery(
    Guid? Id,
    string? Country,
    string? Name,
    string? ServiceType,
    string? SortyBy,
    double MinimumRating = 0.0,
    OrganizationStatus Status = OrganizationStatus.Active,
    int Page = 1,
    int PageSize = 20) : IQuery<PaginatedModel<OrganizationsResponse>>;