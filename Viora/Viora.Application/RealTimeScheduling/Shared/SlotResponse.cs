namespace Viora.Application.RealTimeScheduling.Shared;

public class SlotResponse
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Guid AppointmentId { get; set; }

    public SlotResponse(Guid appointmentId, DateTime startTime, DateTime endTime)
    {

        StartTime = startTime;
        EndTime = endTime;
        AppointmentId = appointmentId;
    }

}
