namespace Viora.Domain.RealTimeScheduling;

public interface IScheduleDelayRepository
{
    public void Add(ScheduleDelay scheduleDelays);
    public void AddAll(IEnumerable<ScheduleDelay> scheduleDelays);
}
