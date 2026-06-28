namespace Viora.Api.Controllers.RealTimeSchedule;

public class GetStaffShiftByDayRequest
{
    public Guid StaffId { get; set; }
    public DateTime DayOfWeek { get; set; }
}
