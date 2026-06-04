namespace Viora.Application.RealTimeScheduling.Shared;

public class StaffShiftResponse
{
    public Guid Id { get; set; }
    public Guid StaffId { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Day { get; set; }



    public StaffShiftResponse(Guid id, Guid staffId, TimeSpan startTime, TimeSpan endTime, string day)
    {
        Id = id;
        StaffId = staffId;
        StartTime = startTime;
        EndTime = endTime;
        Day = day;
    }
}
