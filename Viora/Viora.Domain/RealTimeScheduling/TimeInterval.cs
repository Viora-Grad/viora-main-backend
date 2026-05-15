using Viora.Domain.Abstractions;

namespace Viora.Domain.RealTimeScheduling;

public class TimeInterval : Entity
{
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public TimeInterval()
    {
        // For EF Core
    }
    private TimeInterval(Guid id, TimeSpan startTime, TimeSpan endTime) : base(id)
    {
        StartTime = startTime;
        EndTime = endTime;
    }
}
