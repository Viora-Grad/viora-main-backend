using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.RealTimeScheduling.CancelSchedule;

public record CancelScheduleCommand(Guid StaffId, Guid ShiftId, Guid branchId, DateTime date) : ICommand;

