namespace Viora.Application.RealTimeScheduling.Shared;

public class ShiftResponse
{
    public Guid StaffId { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }


    public ShiftResponse(Guid staffId, TimeOnly startTime, TimeOnly endTime)
    {
        StaffId = staffId;
        StartTime = startTime;
        EndTime = endTime;
    }
}
