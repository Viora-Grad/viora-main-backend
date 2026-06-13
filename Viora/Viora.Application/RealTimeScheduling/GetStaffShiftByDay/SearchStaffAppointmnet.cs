using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;

namespace Viora.Application.RealTimeScheduling.GetStaffShiftByDay;

internal class SearchStaffAppointmentspecification : BaseSpecification<Appointment>
{
    public SearchStaffAppointmentspecification(SearchStaffAppointmentParameters parameters)
    {
        if (parameters.Day.HasValue)
            AddCriteria(a => a.ReservationDate == parameters.Day);
        if (parameters.StaffId.HasValue)
            AddCriteria(a => a.StaffId == parameters.StaffId);
    }
}
internal record SearchStaffAppointmentParameters(
    Guid? StaffId = null,
    DateTime? Day = null
);