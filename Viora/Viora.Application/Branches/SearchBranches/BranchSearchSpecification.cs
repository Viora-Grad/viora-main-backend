using NetTopologySuite.Geometries;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Branches.Internals;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Shared;

namespace Viora.Application.Branches.SearchBranches;

internal sealed class BranchSearchSpecification : BaseSpecification<Branch>
{
    public BranchSearchSpecification(BranchSearchParameters p)
    {
        if (p.BranchId.HasValue)
            AddCriteria(b => b.Id == p.BranchId.Value);

        if (p.OrganizationId.HasValue)
            AddCriteria(b => b.OrganizationId == p.OrganizationId.Value);

        AddCriteria(b => b.Status == p.Status);

        if (p.Services is { Count: > 0 })
            AddCriteria(b => b.ServicesProvided.Any(s => p.Services.Contains(s)));

        // radius filter — only applied when both a point and a radius are given
        if (p.SearchPoint != null && p.DistanceWithinMeters.HasValue)
            AddCriteria(b => b.Location.Distance(p.SearchPoint) <= p.DistanceWithinMeters.Value);

        // ordering — "distance" requires a search point; "rating" is applied post-query in the handler
        bool orderedByDistance = false;
        if (p.OrderBy is not null)
        {
            // TODO add a stamp on the endpoint in the document to show the valid sort options
            foreach (var order in p.OrderBy)
            {
                if (order.Equals("distance", StringComparison.OrdinalIgnoreCase) && p.SearchPoint != null)
                {
                    ApplyOrderBy(b => b.Location.Distance(p.SearchPoint));
                    orderedByDistance = true;
                }
                else if (order.Equals("opened", StringComparison.OrdinalIgnoreCase))
                {
                    ApplyOrderByDescending(b => b.OpenedAtUtc);
                }
                // "rating" is intentionally skipped here — computed post-query in the handler
            }
        }

        // if a search point was provided but distance ordering wasn't explicitly requested,
        // default to ordering by distance so results feel spatially relevant
        if (p.SearchPoint != null && !orderedByDistance)
            ApplyOrderBy(b => b.Location.Distance(p.SearchPoint));

        if (!p.OrderBy?.Any() ?? true)
            ApplyOrderByDescending(b => b.OpenedAtUtc);

        ApplyPaging((p.Page - 1) * p.PageSize, p.PageSize);
    }
}

public sealed class OrganizationByIdsSpecification : BaseSpecification<Organization>
{
    public OrganizationByIdsSpecification(IEnumerable<Guid> ids)
    {
        AddCriteria(o => ids.Contains(o.Id));
    }
}


internal sealed record BranchSearchParameters(
    Guid? BranchId = null,
    Guid? OrganizationId = null,
    BranchStatus Status = BranchStatus.Active,
    IReadOnlyList<ServiceType>? Services = null,
    Point? SearchPoint = null,
    double? DistanceWithinMeters = null,
    IEnumerable<string>? OrderBy = null,
    int Page = 1,
    int PageSize = 20);
