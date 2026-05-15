using Viora.Domain.Abstractions;
using Viora.Domain.RealTimeScheduling.Internals;

namespace Viora.Domain.RealTimeScheduling;

public class ScheduleDelays : Entity
{
    public Guid AppointmentId { get; private set; }
    public TimeSpan DelayDuration { get; private set; }
    public string Reason { get; private set; }
    public DateTime OccurrenceTime { get; private set; }
    public InitiatorType Initiator { get; private set; }

    //public virtual Appointment Appointment { get; private set; }

    public ScheduleDelays()
    {
        // For EF Core
    }

    private ScheduleDelays(Guid id, Guid appointmentId, TimeSpan delayDuration, string reason, DateTime occurrenceTime, InitiatorType initiator) : base(id)
    {
        AppointmentId = appointmentId;
        DelayDuration = delayDuration;
        Reason = reason;
        OccurrenceTime = occurrenceTime;
        Initiator = initiator;
    }
}
