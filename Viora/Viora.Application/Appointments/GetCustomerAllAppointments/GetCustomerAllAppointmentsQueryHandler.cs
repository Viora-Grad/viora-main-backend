using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;
using Viora.Application.Appointments.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;

namespace Viora.Application.Appointments.GetCustomerAllAppointments;

internal class GetCustomerAllAppointmentsQueryHandler(
    IUserContext userContext,
    IAppointmentsRepository appointmentsRepository) : IQueryHandler<GetCustomerAllAppointmentsQuery, PaginatedModel<AppointmentsResponse>>
{
    public async Task<Result<PaginatedModel<AppointmentsResponse>>> Handle(GetCustomerAllAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        var parameters = new GetAppointmentsParameters(
            CustomerId: userId,
            BranchId: request.BranchId,
            ServiceId: request.ServiceId,
            StaffId: request.StaffId,
            PaymentId: request.PaymentId,
            PaymentMethods: request.PaymentMethods,
            CustomerStatus: request.CustomerStatus,
            IncludeCustomerObject: request.IncludeCustomerObject,
            IncludeStaffObject: request.IncludeStaffObject,
            IncludeServiceObject: request.IncludeServiceObject,
            IncludeBranchObject: request.IncludeBranchObject,
            ReservationDate: request.ReservationDate,
            FromDate: request.FromDate,
            ToDate: request.ToDate,
            Page: request.Page,
            PageSize: request.PageSize
        );
        var specs = new GetAppointmentsSpecification(parameters);
        var appointments = await appointmentsRepository.ListAsync(specs, cancellationToken);
        var response = appointments.Select(a => new AppointmentsResponse
        {
            AppointmentId = a.Id,
            ServiceId = a.ServiceId,
            StaffId = a.StaffId,
            BranchId = a.BranchId,
            PaymentId = a.PaymentId,
            PaymentMethod = a.PayMethod.ToString(),
            ReservationDate = a.ReservationDate,
            Status = a.Status.ToString(),
            EstimatedDurationMinutes = a.EstimatedDurationMinutes,
            ServiceName = a.Service?.Name ?? string.Empty,
            StaffName = $"", // staff not implemented yet
            Cost = $"{a.Service?.Cost.ToString()}"
        }).ToList();
        return Result.Success(new PaginatedModel<AppointmentsResponse>(response, request.Page, request.PageSize, response.Count));
    }
}
