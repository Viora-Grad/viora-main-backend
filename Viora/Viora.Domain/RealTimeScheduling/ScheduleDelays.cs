using Viora.Domain.Abstractions;
using Viora.Domain.RealTimeScheduling.Internals;

namespace Viora.Domain.RealTimeScheduling;

public class ScheduleDelay : Entity
{
    public Guid AppointmentId { get; private set; }
    public TimeOnly DelayDuration { get; private set; }
    public string Reason { get; private set; }
    public DateTime OccurrenceTime { get; private set; }
    public InitiatorType Initiator { get; private set; }

    //public virtual Appointment Appointment { get; private set; }

    public ScheduleDelay()
    {
        // For EF Core
    }

    private ScheduleDelay(Guid id, Guid appointmentId, TimeOnly delayDuration, string reason, DateTime occurrenceTime, InitiatorType initiator) : base(id)
    {
        AppointmentId = appointmentId;
        DelayDuration = delayDuration;
        Reason = reason;
        OccurrenceTime = occurrenceTime;
        Initiator = initiator;
    }



    public static ScheduleDelay Create(Guid appointmentId, TimeOnly delayDuration, string reason, DateTime occurrenceTime, InitiatorType initiator)
    {
        return new ScheduleDelay(Guid.NewGuid(), appointmentId, delayDuration, reason, occurrenceTime, initiator);
    }
}
