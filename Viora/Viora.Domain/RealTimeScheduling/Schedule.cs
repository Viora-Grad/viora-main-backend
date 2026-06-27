using Viora.Domain.Abstractions;

namespace Viora.Domain.RealTimeScheduling;

public class Schedule : Entity
{
    public Guid BranchId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    private readonly List<Shift> _intervals;
    public IReadOnlyCollection<Shift> Intervals => _intervals.AsReadOnly();



    public Schedule()
    {
        // For EF Core
    }

    private Schedule(Guid id, Guid branchId, DayOfWeek dayOfWeek) : base(id)
    {
        BranchId = branchId;
        DayOfWeek = dayOfWeek;
    }


    public static Schedule Create(Guid branchid, DayOfWeek dayOfWeek)
    {
        var id = Guid.NewGuid();
        var schedule = new Schedule(id, branchid, dayOfWeek);
        return schedule;
    }
}
