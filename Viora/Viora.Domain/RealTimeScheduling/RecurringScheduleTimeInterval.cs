using Viora.Domain.Abstractions;

namespace Viora.Domain.RealTimeScheduling;

public class RecurringScheduleTimeInterval : Entity
{
    public Guid RecurringScheduleId { get; private set; }
    public Guid TimeInterval { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public Guid StaffId { get; private set; }
    public virtual RecurringSchedule RecurringSchedule { get; private set; }
    public virtual TimeInterval TimeIntervalEntity { get; private set; }

    public RecurringScheduleTimeInterval()
    {
        // For EF Core
    }

    private RecurringScheduleTimeInterval(Guid id, Guid recurringScheduleId, Guid timeInterval, DayOfWeek dayOfWeek, Guid staffId) : base(id)
    {
        RecurringScheduleId = recurringScheduleId;
        TimeInterval = timeInterval;
        DayOfWeek = dayOfWeek;
        StaffId = staffId;
    }


}
