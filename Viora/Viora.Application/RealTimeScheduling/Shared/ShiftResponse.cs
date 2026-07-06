namespace Viora.Application.RealTimeScheduling.Shared;

public class ShiftResponse
{
    public Guid ShiftId { get; set; }
    public Guid StaffId { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }


    public ShiftResponse(Guid shiftId, Guid staffId, TimeOnly startTime, TimeOnly endTime)
    {
        ShiftId = shiftId;
        StaffId = staffId;
        StartTime = startTime;
        EndTime = endTime;
    }
}
