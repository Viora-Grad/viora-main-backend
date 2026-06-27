using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;

namespace Viora.Application.Appointments.Shared;

internal class GetAppointmentsSpecification : BaseSpecification<Appointment>
{
    public GetAppointmentsSpecification(GetAppointmentsParameters p)
    {
        if (p.Id.HasValue)
            AddCriteria(a => a.Id == p.Id);

        if (p.CustomerId.HasValue)
            AddCriteria(a => a.CustomerId == p.CustomerId);

        if (p.BranchId.HasValue)
            AddCriteria(a => a.BranchId == p.BranchId);

        if (p.ServiceId.HasValue)
            AddCriteria(a => a.ServiceId == p.ServiceId);

        if (p.StaffId.HasValue)
            AddCriteria(a => a.StaffId == p.StaffId);

        if (p.PaymentId.HasValue)
            AddCriteria(a => a.PaymentId == p.PaymentId);

        if (p.CustomerStatus != null && p.CustomerStatus.Any())
            AddCriteria(a => p.CustomerStatus.Contains(a.Status.ToString()));

        if (p.PaymentMethods != null && p.PaymentMethods.Any())
            AddCriteria(a => p.PaymentMethods.Contains(a.PayMethod.ToString()));

        if (p.ReservationDate.HasValue)
            AddCriteria(a => a.ReservationDate.Date == p.ReservationDate.Value.Date);

        if (p.FromDate.HasValue)
            AddCriteria(a => a.ReservationDate >= p.FromDate);

        if (p.ToDate.HasValue)
            AddCriteria(a => a.ReservationDate <= p.ToDate);

        if (p.IncludeCustomerObject == true)
            AddInclude(a => a.Customer!);

        if (p.IncludeStaffObject == true)
            AddInclude(a => a.Staff);

        if (p.IncludeServiceObject == true)
            AddInclude(a => a.Service);

        if (p.IncludeBranchObject == true)
            AddInclude(a => a.Branch);

        if (p.Page > 0 && p.PageSize > 0)
            ApplyPaging((p.Page - 1) * p.PageSize, p.PageSize);

        ApplyOrderByDescending(a => a.ReservationDate);

    }
}

public sealed record GetAppointmentsParameters(
    Guid? Id = null,
    Guid? CustomerId = null,
    Guid? BranchId = null,
    Guid? ServiceId = null,
    Guid? StaffId = null,
    Guid? PaymentId = null,
    IEnumerable<string>? CustomerStatus = null,
    IEnumerable<string>? PaymentMethods = null,
    bool? IncludeCustomerObject = false,
    bool? IncludeStaffObject = false,
    bool? IncludeServiceObject = false,
    bool? IncludeBranchObject = false,
    DateTime? ReservationDate = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 20);