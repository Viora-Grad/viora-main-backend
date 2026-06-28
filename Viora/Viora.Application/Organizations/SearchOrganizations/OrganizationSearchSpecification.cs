using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Organizations.OrganizationDetails.Internal;
using Viora.Domain.Shared;

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
            AddCriteria(o => o.Name.Value.Contains(p.Name));

        if (p.MinRating.HasValue)
            AddCriteria(o => o.Rating.AverageOutOfTen >= p.MinRating.Value);

        if (p.ServiceType != null)
            AddCriteria(o => o.ServicesProvided.Contains(p.ServiceType));

        AddCriteria(o => o.Status == p.Status);

        switch (p.SortBy?.ToLower())
        {
            case string s when s.Equals(nameof(Organization.Rating), StringComparison.OrdinalIgnoreCase):
                ApplyOrderByDescending(o => o.Rating.AverageOutOfTen);
                break;

            case string s when s.Equals(nameof(Organization.Name), StringComparison.OrdinalIgnoreCase):
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
    OrganizationStatus Status = OrganizationStatus.Active,
    string? SortBy = null,
    int Page = 1,
    int PageSize = 20);