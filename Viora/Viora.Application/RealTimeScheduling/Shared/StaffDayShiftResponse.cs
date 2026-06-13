namespace Viora.Application.RealTimeScheduling.Shared;

public class StaffDayShiftResponse
{
    public Guid StaffId { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public List<SlotResponse> TimeReserved { get; set; } = new List<SlotResponse>();


    public StaffDayShiftResponse(Guid staffId, TimeOnly startTime, TimeOnly endTime, List<SlotResponse> timeReserved)
    {
        StaffId = staffId;
        StartTime = startTime;
        EndTime = endTime;
        TimeReserved = timeReserved;
    }

}
