using Viora.Domain.RealTimeScheduling;

namespace Viora.Infrastructure.Seeding.Data;

internal static class ScheduleData
{
    public static IReadOnlyList<Schedule> All { get; } =
    [
        Schedule.Create(new Guid("9D88C6E0-7B53-434A-82AC-83F1AB9B5C19"),DayOfWeek.Monday),
        Schedule.Create(new Guid("9D88C6E0-7B53-434A-82AC-83F1AB9B5C19"),DayOfWeek.Tuesday),
        Schedule.Create(new Guid("9D88C6E0-7B53-434A-82AC-83F1AB9B5C19"),DayOfWeek.Wednesday),
        Schedule.Create(new Guid("9D88C6E0-7B53-434A-82AC-83F1AB9B5C19"),DayOfWeek.Thursday),
        Schedule.Create(new Guid("9D88C6E0-7B53-434A-82AC-83F1AB9B5C19"),DayOfWeek.Friday),

    ];
}
