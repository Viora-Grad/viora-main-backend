namespace Viora.Application.RealTimeScheduling.Shared;

public class StaffDayShiftResponse
{
    public Guid ShiftId { get; set; }
    public Guid ScheduleId { get; set; }
    public Guid StaffId { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public List<SlotResponse> TimeReserved { get; set; } = new List<SlotResponse>();


    public StaffDayShiftResponse(Guid shiftId, Guid scheduleId, Guid staffId, TimeOnly startTime, TimeOnly endTime, List<SlotResponse> timeReserved)
    {
        ShiftId = shiftId;
        ScheduleId = scheduleId;
        StaffId = staffId;
        StartTime = startTime;
        EndTime = endTime;
        TimeReserved = timeReserved;
    }

}
