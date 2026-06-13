using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;
using Viora.Domain.Abstractions;

namespace Viora.Application.Appointments.GetCustomerAppointments;

internal class GetCustomerAppointmentsQueryHandler() : IQueryHandler<GetCustomerAppointmentsQuery, PaginatedModel<GetCustomerAppointmentsResponse>>
{
    public Task<Result<PaginatedModel<GetCustomerAppointmentsResponse>>> Handle(GetCustomerAppointmentsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
