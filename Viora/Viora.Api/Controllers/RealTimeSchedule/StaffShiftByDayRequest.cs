namespace Viora.Api.Controllers.RealTimeSchedule;

public class StaffShiftByDayRequest
{
    public DateTime day { get; set; }
    public Guid StaffId { get; set; }
    public Guid ShiftId { get; set; }
}
