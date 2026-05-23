using Viora.Domain.RealTimeScheduling;

namespace Viora.Application.RealTimeScheduling.Shared;

public class BranchScheduleResponse
{
    public string? Day { get; set; }

    public List<ShiftResponse> Shifts { get; set; }

    public BranchScheduleResponse(string day, List<ShiftResponse> shifts)
    {
        Day = day;
        Shifts = shifts;
    }

    public static List<BranchScheduleResponse> MapToList(List<Schedule> schedules)
    {
        var schedulesResponse = schedules
                .Select(schedule => new BranchScheduleResponse(
                         schedule.DayOfWeek.ToString(),
                         schedule.Intervals
                             .Select(shift => new ShiftResponse(
                                 shift.StaffId,
                                 shift.StartTime,
                                 shift.EndTime
                             )).ToList()
                     )).ToList();

        return schedulesResponse;
    }

}
