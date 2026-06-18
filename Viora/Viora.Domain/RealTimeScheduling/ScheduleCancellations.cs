using Viora.Domain.Abstractions;

namespace Viora.Domain.RealTimeScheduling;

public class ScheduleCancellations : Entity
{
    public Guid ShiftId { get; private set; }
    public DateTime CancellationDate { get; private set; }
    public string Reason { get; private set; }

    public ScheduleCancellations()
    {
        // For EF Core
    }
    private ScheduleCancellations(Guid id, Guid shiftId, DateTime cancellationDate, string reason) : base(id)
    {
        ShiftId = shiftId;
        CancellationDate = cancellationDate;
        Reason = reason;
    }


    public static ScheduleCancellations Create(Guid shiftId, DateTime cancellationDate, string reason)
    {
        var id = Guid.NewGuid();
        var newCancellation = new ScheduleCancellations(id, shiftId, cancellationDate, reason);
        return newCancellation;
    }
}
