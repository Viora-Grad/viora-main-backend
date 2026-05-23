using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.RealTimeScheduling.CreateRecurringSchedule;

public record CreateShiftCommand(
    Guid BranchId,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string DayOfWeek,
    Guid StaffId
    ) : ICommand<Guid>;
