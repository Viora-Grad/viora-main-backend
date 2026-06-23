using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Internal;

namespace Viora.Application.RealTimeScheduling.Shared;

internal class SearchShiftAppointmentSpecification : BaseSpecification<Appointment>
{
    public SearchShiftAppointmentSpecification(SearchShiftAppoinmentparameter parameters)
    {
        if (parameters.StaffId.HasValue)
            AddCriteria(x => x.StaffId == parameters.StaffId);
        if (parameters.StartDate.HasValue && parameters.EndDate.HasValue)
            AddCriteria(x => x.ReservationDate >= parameters.StartDate && x.ReservationDate <= parameters.EndDate);
        AddCriteria(x => x.Status == CustomerStatus.Waiting || x.Status == CustomerStatus.NotArrived);

    }
}


public record SearchShiftAppoinmentparameter(
    Guid? StaffId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
    );