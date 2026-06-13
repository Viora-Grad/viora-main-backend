using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;

namespace Viora.Application.Appointments.GetCustomerAppointments;

internal class GetCustomerAppointmentSpecification : BaseSpecification<Appointment>
{
    public GetCustomerAppointmentSpecification(CustomerAppointmentsParameters p)
    {
        if (p.CustomerId != Guid.Empty)
            AddCriteria(a => a.CustomerId == p.CustomerId);

        if (p.Page > 0 && p.PageSize > 0)
            ApplyPaging((p.Page - 1) * p.PageSize, p.PageSize);

        if (p.IncludeStaffObject)
            AddInclude(a => a.Staff);
        if (p.IncludeServiceObject)
            AddInclude(a => a.Service);

        ApplyOrderByDescending(a => a.ReservationDate);

    }
}

public sealed record CustomerAppointmentsParameters(
    Guid CustomerId,
    bool IncludeStaffObject = false,
    bool IncludeServiceObject = false,
    int Page = 1,
    int PageSize = 20);