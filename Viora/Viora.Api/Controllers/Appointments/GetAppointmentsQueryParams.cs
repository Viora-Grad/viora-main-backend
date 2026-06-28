namespace Viora.Api.Controllers.Appointments;

public sealed record GetAppointmentsQueryParams(
    Guid? CustomerId = null,
    Guid? BranchId = null,
    Guid? ServiceId = null,
    Guid? StaffId = null,
    IEnumerable<string>? CustomerStatus = null,
    bool? IncludeCustomerObject = false,
    bool? IncludeStaffObject = false,
    bool? IncludeServiceObject = false,
    bool? IncludeBranchObject = false,
    DateTime? ReservationDate = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 20
    );
