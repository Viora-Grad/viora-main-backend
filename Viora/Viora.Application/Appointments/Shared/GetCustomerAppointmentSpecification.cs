using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Internal;

namespace Viora.Application.Appointments.Shared;

internal class GetCustomerAppointmentSpecification : BaseSpecification<Appointment>
{
    public GetCustomerAppointmentSpecification(CustomerAppointmentsParameters p)
    {
        if (p.CustomerId != Guid.Empty)
            AddCriteria(a => a.CustomerId == p.CustomerId);

        if (p.Page > 0 && p.PageSize > 0)
            ApplyPaging((p.Page - 1) * p.PageSize, p.PageSize);

        if (p.IncludeStaffObject == true)
            AddInclude(a => a.Staff);
        if (p.IncludeServiceObject == true)
            AddInclude(a => a.Service);
        if (p.IncludeBranchObject == true)
            AddInclude(a => a.Branch);
        if (p.IncludeCancelled == false)
            AddCriteria(a => a.Status != CustomerStatus.Canceled);
        if (p.IncludeCompleted == false)
            AddCriteria(a => a.Status != CustomerStatus.Completed);

        ApplyOrderByDescending(a => a.ReservationDate);

    }
}

public sealed record CustomerAppointmentsParameters(
    Guid? CustomerId = null,
    bool? IncludeCancelled = false,
    bool? IncludeCompleted = false,
    bool? IncludeStaffObject = false,
    bool? IncludeServiceObject = false,
    bool? IncludeBranchObject = false,
    int Page = 1,
    int PageSize = 20);