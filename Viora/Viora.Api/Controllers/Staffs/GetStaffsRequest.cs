namespace Viora.Api.Controllers.Staffs;

public sealed record GetStaffsRequest(
    Guid? StaffId = null,
    Guid? OrganizationId = null,
    string? FirstName = null,
    string? LastName = null,
    IEnumerable<int>? RoleIds = null,
    IEnumerable<Guid>? BranchIds = null,
    IEnumerable<Guid>? ServiceIds = null,
    string? Gender = null,
    IEnumerable<string>? Statuses = null,
    int Page = 1,
    int PageSize = 20
    );