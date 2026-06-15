using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;

namespace Viora.Application.Appointments.GetCustomerAppointments;

public sealed record GetCustomerAllAppointmentsQuery(
    bool IncludeCancelled,
    bool IncludeCompleted,
    bool IncludeStaffObject,
    bool IncludeServiceObject,
    bool IncludeBranchObject,
    int Page = 1,
    int PageSize = 20) : IQuery<PaginatedModel<GetCustomerAllAppointmentsResponse>>;