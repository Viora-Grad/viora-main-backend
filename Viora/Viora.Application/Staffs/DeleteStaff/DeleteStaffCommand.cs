
using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Staffs.DeleteStaff;

public sealed record DeleteStaffCommand(Guid Id) : ICommand;
