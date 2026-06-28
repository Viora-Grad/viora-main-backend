using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;
using Viora.Domain.Branches.Internals;

namespace Viora.Application.Branches.SearchBranches;

public sealed record SearchBranchesQuery(
    Guid? BranchId,
    Guid? OrganizationId,
    double? Longitude,
    double? Latitude,
    bool? IsCurrentlyOpen,
    IEnumerable<string>? ServicesFilter = null,
    IEnumerable<string>? OrderBy = null,
    BranchStatus Status = BranchStatus.Active,
    double? DistanceWithinMeters = null,
    double MinimumRating = 0.0,
    int Page = 1,
    int PageSize = 20) : IQuery<PaginatedModel<SearchBranchesResponse>>;