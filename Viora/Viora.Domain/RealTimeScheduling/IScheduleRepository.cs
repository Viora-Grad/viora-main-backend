namespace Viora.Domain.RealTimeScheduling;

public interface IScheduleRepository
{
    public Task<Schedule> getByBranchIdAndDayAsync(Guid branchId, DayOfWeek dayOfWeek, CancellationToken cancellationToken);
    public Task<List<Schedule>> getByBranchIdAsync(Guid branchId, CancellationToken cancellationToken);
    public void Add(Schedule schedule);
}
