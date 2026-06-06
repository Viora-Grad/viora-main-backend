using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;

namespace Viora.Application.Organizations.SearchOrganizations;

public record SearchOrganizationsQuery(
    Guid? Id,
    string? Country,
    string? Name,
    string? ServiceType,
    double MinimumRating = 0.0,
    int Page = 1,
    int PageSize = 20) : IQuery<PaginatedModel<OrganizationsResponse>>;