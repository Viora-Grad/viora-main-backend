using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;
using Viora.Application.Appointments.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;

namespace Viora.Application.Appointments.GetCustomerAppointments;

internal class GetCustomerAllAppointmentsQueryHandler(
    IUserContext userContext,
    IAppointmentsRepository appointmentsRepository) : IQueryHandler<GetCustomerAllAppointmentsQuery, PaginatedModel<GetCustomerAllAppointmentsResponse>>
{
    public async Task<Result<PaginatedModel<GetCustomerAllAppointmentsResponse>>> Handle(GetCustomerAllAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        var parameters = new CustomerAppointmentsParameters(userId);
        var specs = new GetCustomerAppointmentSpecification(parameters);
        var appointments = await appointmentsRepository.ListAsync(specs, cancellationToken);
        var response = appointments.Select(a => new GetCustomerAllAppointmentsResponse
        {
            AppointmentId = a.Id,
            ServiceId = a.ServiceId,
            StaffId = a.StaffId,
            BranchId = a.BranchId,
            ReservationDate = a.ReservationDate,
            Status = a.Status,
            EstimatedDuration = a.EstimatedDuration,
            ServiceName = a.Service?.Name ?? string.Empty,
            StaffName = $"", // staff not implemented yet
            Cost = $"{a.Service?.Cost.Amount}{a.Service?.Cost.Currency}"
        }).ToList();
        return Result.Success(new PaginatedModel<GetCustomerAllAppointmentsResponse>(response, request.Page, request.PageSize, response.Count));
    }
}
