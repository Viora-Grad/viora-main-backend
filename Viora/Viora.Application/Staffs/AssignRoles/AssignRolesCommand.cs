using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Staffs.AssignRoles;

public sealed record AssignRolesCommand(
    Guid StaffId,
    List<int> RoleIds) : ICommand;
