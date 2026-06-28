using Viora.Domain.Abstractions;

namespace Viora.Domain.RealTimeScheduling;

public class ScheduleError
{
    public static readonly Error ShiftOverlap = new Error("shiftOverLap", "shift overlap with existance shift", ErrorCategory.Conflict);
    public static readonly Error NotFoundForDay = new Error("NotFoundForDay", "there are not schedule in this day", ErrorCategory.NotFound);
    public static readonly Error ScheduleOverLap = new Error("ScheduleOverLap", "schedule overlap with existance schedule", ErrorCategory.Conflict);
    public static readonly Error ScheduleNotFound = new Error("ScheduleNotFound", "branch does not have schedule", ErrorCategory.NotFound);
    public static readonly Error ShiftsNotFound = new Error("ShiftsNotFound", "this staff does not have shifts ", ErrorCategory.NotFound);

}
