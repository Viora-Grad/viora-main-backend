using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Staffs.UpdateServices;

public sealed record UpdateServicesCommand(
    Guid StaffId,
    List<Guid> ServiceIds
) : ICommand;
