using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;

namespace Viora.Application.Staffs.SearchStaff;

public sealed record SearchStaffQuery(
    Guid? StaffId = null,
    Guid? OrganizationId = null,
    string? FirstName = null,
    string? LastName = null,
    string? Gender = null,
    IEnumerable<string>? Statuses = null,
    IEnumerable<int>? RoleIds = null,
    IEnumerable<Guid>? BranchIds = null,
    IEnumerable<Guid>? ServiceIds = null,
    int Page = 1,
    int PageSize = 20
    ) : IQuery<PaginatedModel<SearchStaffResponse>>;
