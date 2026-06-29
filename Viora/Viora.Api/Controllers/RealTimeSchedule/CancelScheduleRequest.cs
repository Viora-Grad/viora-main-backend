namespace Viora.Api.Controllers.RealTimeSchedule;

public class CancelScheduleRequest
{
    public Guid ShiftId { get; set; }
    public Guid BranchId { get; set; }
    public DateTime cancellationDate { get; set; }
    public string Reason { get; set; }

}
