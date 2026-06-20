using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.RealTimeScheduling.CreateSchedule;

public record CreateScheduleCommand(
    Guid BranchId,
    string DayOfWeek) : ICommand<Guid>;
