namespace Viora.Api.Controllers.Staffs;

public sealed record AssignRolesRequest(ICollection<int> RoleIds);

