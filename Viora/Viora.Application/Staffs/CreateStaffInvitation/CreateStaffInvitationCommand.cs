using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Staffs.CreateStaffInvitation;

public sealed record CreateStaffInvitationCommand(
    Guid OrganizationId,
    List<int> RoleIds,
    List<Guid> BranchIds
    ) : ICommand<string>;

