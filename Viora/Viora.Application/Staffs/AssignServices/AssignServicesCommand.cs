using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Staffs.AssignServices;

public sealed record AssignServicesCommand(
    Guid StaffId,
    List<Guid> ServiceIds
) : ICommand;