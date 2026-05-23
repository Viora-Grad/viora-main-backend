namespace Viora.Application.RealTimeScheduling.Shared;

public class StaffDayShiftResponse
{
    public Guid StaffId { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public List<TimeSpan> TimeReserved { get; set; } = new List<TimeSpan>();


    public StaffDayShiftResponse(Guid staffId, TimeSpan startTime, TimeSpan endTime, List<TimeSpan> timeReserved)
    {
        StaffId = staffId;
        StartTime = startTime;
        EndTime = endTime;
        TimeReserved = timeReserved;
    }

}
