using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Shared.Enums;

namespace Viora.Application.Organizations.SearchOrganizations;

internal class OrganizationSearchSpecification : BaseSpecification<Organization>
{
    public OrganizationSearchSpecification(OrganizationSearchParameters p)
    {
        if (p.Id.HasValue)
            AddCriteria(o => o.Id == p.Id.Value);

        if (p.CountryId.HasValue)
            AddCriteria(o => o.CountryId == p.CountryId.Value);

        if (!string.IsNullOrWhiteSpace(p.Name))
            AddCriteria(o => o.Name.Value.Contains(p.Name, StringComparison.CurrentCultureIgnoreCase));

        if (p.MinRating.HasValue)
            AddCriteria(o => o.Rating.AverageOutOfTen >= p.MinRating.Value);

        if (p.ServiceType.HasValue)
            AddCriteria(o => o.ServicesProvided.Contains(p.ServiceType.Value));

        switch (p.SortBy?.ToLower())
        {
            case nameof(Organization.Rating):
                ApplyOrderByDescending(o => o.Rating.AverageOutOfTen);
                break;
            case nameof(Organization.Name):
                ApplyOrderBy(o => o.Name);
                break;
            default:
                ApplyOrderByDescending(o => o.JoinedOnUtc);
                break;
        }

        ApplyPaging((p.Page - 1) * p.PageSize, p.PageSize);
    }
}

internal record OrganizationSearchParameters(
    Guid? Id = null,
    Guid? CountryId = null,
    string? Name = null,
    double? MinRating = null,
    ServiceType? ServiceType = null,
    string? SortBy = null,
    int Page = 1,
    int PageSize = 20);