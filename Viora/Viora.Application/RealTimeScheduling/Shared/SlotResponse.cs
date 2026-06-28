namespace Viora.Application.RealTimeScheduling.Shared;

public class SlotResponse
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public SlotResponse(DateTime startTime, DateTime endTime)
    {
        StartTime = startTime;
        EndTime = endTime;
    }

}
