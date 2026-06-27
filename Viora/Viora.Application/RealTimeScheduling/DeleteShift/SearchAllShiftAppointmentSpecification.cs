using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Internal;

namespace Viora.Application.RealTimeScheduling.DeleteShift;

public class SearchAllShiftAppointmentSpecification : BaseSpecification<Appointment>
{
    public SearchAllShiftAppointmentSpecification(SearchAllShiftAppointmentParameter parameters)
    {
        if (parameters.StaffId.HasValue)
            AddCriteria(x => x.StaffId == parameters.StaffId);
        {
            TimeSpan startTime = parameters.StartTime.Value.ToTimeSpan();
            TimeSpan endTime = parameters.EndTime.Value.ToTimeSpan();

            AddCriteria(x =>
                x.ReservationDate.TimeOfDay >= startTime &&
                x.ReservationDate.TimeOfDay <= endTime);
        }
        AddCriteria(x => x.Status != CustomerStatus.Canceled && x.Status != CustomerStatus.Completed && x.Status != CustomerStatus.InProgress);
    }

}


public record SearchAllShiftAppointmentParameter(
    Guid? StaffId = null,
    TimeOnly? StartTime = null,
    TimeOnly? EndTime = null
);
