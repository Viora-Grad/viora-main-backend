namespace Viora.Domain.RealTimeScheduling;

public interface IShiftRepository
{
    public Task<Shift?> GetActiveShiftAsync(Guid ScheduleId, Guid Staff, TimeOnly time, CancellationToken cancellationToken);
    public void Add(Shift shift);
    public Task<Shift?> GetByIdAsync(Guid Id, CancellationToken cancellationToken);
}
