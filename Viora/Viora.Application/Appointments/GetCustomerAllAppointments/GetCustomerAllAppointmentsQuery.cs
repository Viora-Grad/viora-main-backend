using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;
using Viora.Application.Appointments.Shared;

namespace Viora.Application.Appointments.GetCustomerAllAppointments;

public sealed record GetCustomerAllAppointmentsQuery(
    Guid CustomerId,
    Guid? BranchId = null,
    Guid? ServiceId = null,
    Guid? StaffId = null,
    Guid? PaymentId = null,
    IEnumerable<string>? PaymentMethods = null,
    IEnumerable<string>? CustomerStatus = null,
    bool? IncludeStaffObject = false,
    bool? IncludeServiceObject = false,
    bool? IncludeBranchObject = false,
    bool? IncludeCustomerObject = false,
    DateTime? ReservationDate = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 20) : IQuery<PaginatedModel<AppointmentsResponse>>;