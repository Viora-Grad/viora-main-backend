using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;

namespace Viora.Application.Appointments.GetCustomerAppointments;

public sealed record GetCustomerAppointmentsQuery() : IQuery<PaginatedModel<GetCustomerAppointmentsResponse>>;