using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Staffs.ChangeStatus;

public sealed record ChangeStatusCommand(Guid Id, string Status) : ICommand;