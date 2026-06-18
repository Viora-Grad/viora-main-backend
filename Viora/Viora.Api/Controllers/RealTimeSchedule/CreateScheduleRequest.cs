namespace Viora.Api.Controllers.RealTimeSchedule;

public class CreateScheduleRequest
{
    public Guid BranchId { get; set; }
    public string DayOfWeek { get; set; }
}
