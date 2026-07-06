using Viora.Domain.Abstractions;
using Viora.Domain.Staffs;

namespace Viora.Application.Staffs.SearchStaff;

internal class SearchStaffSpecification : BaseSpecification<Staff>
{
    public SearchStaffSpecification(SearchStaffParameters p)
    {
        if (p.StaffId.HasValue)
            AddCriteria(s => s.Id == p.StaffId);

        if (p.OrganizationId.HasValue)
            AddCriteria(s => s.OrganizationId == p.OrganizationId);

        if (!string.IsNullOrWhiteSpace(p.FirstName))
            AddCriteria(s => s.FirstName == p.FirstName);

        if (!string.IsNullOrWhiteSpace(p.LastName))
            AddCriteria(s => s.LastName == p.LastName);

        if (p.RoleIds != null && p.RoleIds.Any())
            AddCriteria(s => s.Roles.Any(r => p.RoleIds.Contains(r.Id)));

        if (p.BranchIds != null && p.BranchIds.Any())
        {
            AddInclude(s => s.Branches);
            AddCriteria(s => s.Branches.Any(b => p.BranchIds.Contains(b.Id)));
        }

        if (p.ServiceIds != null && p.ServiceIds.Any())
        {
            AddInclude(s => s.Services);
            AddCriteria(s => s.Services.Any(sv => p.ServiceIds.Contains(sv.Id)));
        }

        if (!string.IsNullOrWhiteSpace(p.Gender))
            AddCriteria(s => s.Gender.ToString() == p.Gender);

        if (p.Statuses != null && p.Statuses.Any())
            AddCriteria(s => p.Statuses.Contains(s.StaffStatus.ToString()));

        if (p.Page > 0 && p.PageSize > 0)
            ApplyPaging((p.Page - 1) * p.PageSize, p.PageSize);
    }
}

public sealed record SearchStaffParameters(
    Guid? StaffId = null,
    Guid? OrganizationId = null,
    string? FirstName = null,
    string? LastName = null,
    string? Gender = null,
    IEnumerable<int>? RoleIds = null,
    IEnumerable<Guid>? BranchIds = null,
    IEnumerable<Guid>? ServiceIds = null,
    IEnumerable<string>? Statuses = null,
    int Page = 1,
    int PageSize = 20
    );