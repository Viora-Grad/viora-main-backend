using Viora.Domain.Abstractions;

namespace Viora.Domain.RealTimeScheduling;

public class RecurringSchedule : Entity
{
    public Guid BranchId { get; private set; }
    private readonly List<RecurringScheduleTimeInterval> _intervals;
    public IReadOnlyCollection<RecurringScheduleTimeInterval> Intervals => _intervals.AsReadOnly();



    public RecurringSchedule()
    {
        // For EF Core
    }

    private RecurringSchedule(Guid id, Guid branchId) : base(id)
    {
        BranchId = branchId;
    }
}
