namespace Viora.Domain.RealTimeScheduling;

public interface IScheduleDelayRepository
{
    public void Add(ScheduleDelay scheduleDelays);
    public void AddRange(IEnumerable<ScheduleDelay> scheduleDelays);
}
