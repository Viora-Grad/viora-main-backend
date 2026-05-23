namespace Viora.Application.RealTimeScheduling.Shared;

public class ShiftResponse
{
    public Guid StaffId { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }


    public ShiftResponse(Guid staffId, TimeSpan startTime, TimeSpan endTime)
    {
        StaffId = staffId;
        StartTime = startTime;
        EndTime = endTime;
    }
}
