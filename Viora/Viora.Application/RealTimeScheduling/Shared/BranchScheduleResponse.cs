using Viora.Domain.RealTimeScheduling;

namespace Viora.Application.RealTimeScheduling.Shared;

public class BranchScheduleResponse
{
    public Guid Id { get; set; }
    public string Day { get; set; }

    public List<ShiftResponse> Shifts { get; set; }

    public BranchScheduleResponse(Guid id, string day, List<ShiftResponse> shifts)
    {
        Id = id;
        Day = day;
        Shifts = shifts;
    }

    public static List<BranchScheduleResponse> MapToList(List<Schedule> schedules)
    {
        var schedulesResponse = schedules
                .Select(schedule => new BranchScheduleResponse(
                         schedule.Id,
                         schedule.DayOfWeek.ToString(),
                         schedule.Intervals
                             .Select(shift => new ShiftResponse(
                                 shift.Id,
                                 shift.StaffId,
                                 shift.StartTime,
                                 shift.EndTime
                             )).ToList()
                     )).ToList();

        return schedulesResponse;
    }

}
