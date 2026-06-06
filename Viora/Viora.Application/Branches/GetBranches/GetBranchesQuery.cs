using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;

namespace Viora.Application.Branches.GetBranches;

internal sealed record GetBranchesQuery(
    Guid? BranchId,
    Guid? OrganizationId,
    KeyValuePair<double, double>? Coordinations,
    bool? IsCurrentlyOpen,
    double MinimumRating = 0.0) : IQuery<PaginatedModel<GetBranchesResponse>>;