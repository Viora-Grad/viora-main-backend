using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.RealTimeScheduling.CreateRecurringSchedule;

public record CreateShiftCommand(
    Guid BranchId,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string DayOfWeek,
    Guid StaffId
    ) : ICommand<Guid>;
