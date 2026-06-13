namespace Viora.Application.RealTimeScheduling.Shared;

public class StaffShiftResponse
{
    public Guid Id { get; set; }
    public Guid StaffId { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Day { get; set; }



    public StaffShiftResponse(Guid id, Guid staffId, TimeOnly startTime, TimeOnly endTime, string day)
    {
        Id = id;
        StaffId = staffId;
        StartTime = startTime;
        EndTime = endTime;
        Day = day;
    }
}
