namespace Viora.Api.Controllers.RealTimeSchedule;

public class CreateShiftRequest
{
    public Guid BranchId { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public Guid StaffId { get; set; }
    public string DayOfWeek { get; set; }

}
