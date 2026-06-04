using Viora.Domain.Abstractions;

namespace Viora.Domain.RealTimeScheduling;

public class Shift : Entity
{
    public Guid ScheduleId { get; private set; }
    public Guid StaffId { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }

    //private readonly List<Appointment> _Appointments;
    //public IReadOnlyCollection<Appointment> Appointments=_Appointment.AsReadOnly();

    public Shift()
    {
        // For EF Core
    }

    private Shift(Guid id, TimeOnly startTime, TimeOnly endTime, Guid scheduleId, Guid staffId) : base(id)
    {
        ScheduleId = scheduleId;
        StartTime = startTime;
        EndTime = endTime;
        StaffId = staffId;
    }


    public static Shift Create(Guid scheduleId, TimeOnly startTime, TimeOnly endTime, Guid staffId)
    {
        var id = Guid.NewGuid();
        var newShift = new Shift(id, startTime, endTime, scheduleId, staffId);
        return newShift;
    }

}
