using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;

namespace Viora.Application.RealTimeScheduling.GetStaffShiftByDay;

internal class SearchStaffAppointmentspecification : BaseSpecification<Appointment>
{
    public SearchStaffAppointmentspecification(SearchStaffAppointmentParameters parameters)
    {
        if (parameters.StartDate.HasValue && parameters.EndDate.HasValue)
            AddCriteria(x => x.ReservationDate >= parameters.StartDate && x.ReservationDate <= parameters.EndDate);
        if (parameters.StaffId.HasValue)
            AddCriteria(a => a.StaffId == parameters.StaffId);
    }
}
internal record SearchStaffAppointmentParameters(
    Guid? StaffId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
);