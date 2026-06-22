using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;
using Viora.Application.Appointments.Shared;

namespace Viora.Application.Appointments.GetDoctorAppointments;

public sealed record GetDoctorAppointmentsQuery(
    Guid StaffId,
    Guid? CustomerId = null,
    Guid? BranchId = null,
    Guid? ServiceId = null,
    IEnumerable<string>? CustomerStatus = null,
    bool? IncludeStaffObject = false,
    bool? IncludeServiceObject = false,
    bool? IncludeBranchObject = false,
    DateTime? ReservationDate = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 20) : IQuery<PaginatedModel<AppointmentsResponse>>;
