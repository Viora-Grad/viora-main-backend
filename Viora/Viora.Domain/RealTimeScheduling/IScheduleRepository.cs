namespace Viora.Domain.RealTimeScheduling;

public interface IScheduleRepository
{
    public Task<Schedule?> getByBranchIdAndDayAsync(Guid branchId, DayOfWeek dayOfWeek, CancellationToken cancellationToken);
    public Task<List<Schedule>> getByBranchIdAsync(Guid branchId, CancellationToken cancellationToken);
    //public Task<IReadOnlyList<Schedule>> ListAsync(ISpecification<Schedule> spec, CancellationToken cancellationToken = default);
    public void Add(Schedule schedule);
}
