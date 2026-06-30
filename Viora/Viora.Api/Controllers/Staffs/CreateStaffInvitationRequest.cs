namespace Viora.Api.Controllers.Staffs;

public sealed record CreateStaffInvitationRequest(
    IEnumerable<Guid> BranchIds,
    IEnumerable<int> RoleIds);
