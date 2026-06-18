using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.RealTimeScheduling.CancelSchedule;

public record CancelScheduleCommand(Guid ShiftId, Guid branchId, DateTime date, string reason) : ICommand;

