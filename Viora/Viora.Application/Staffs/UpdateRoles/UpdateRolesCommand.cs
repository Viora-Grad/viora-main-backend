using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Staffs.UpdateRoles;

public sealed record UpdateRolesCommand(Guid StaffId,
    List<int> RoleIds) : ICommand;
