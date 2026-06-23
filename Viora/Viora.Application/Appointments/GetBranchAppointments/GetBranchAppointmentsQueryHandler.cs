using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Pagination;
using Viora.Application.Appointments.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;

namespace Viora.Application.Appointments.GetBranchAppointments;

internal class GetBranchAppointmentsQueryHandler(
    IAppointmentsRepository appointmentsRepository
    ) : IQueryHandler<GetBranchAppointmentsQuery, PaginatedModel<AppointmentsResponse>>
{
    public async Task<Result<PaginatedModel<AppointmentsResponse>>> Handle(GetBranchAppointmentsQuery request, CancellationToken cancellationToken)
    {

        var parameters = new GetAppointmentsParameters(
            CustomerId: request.CustomerId,
            BranchId: request.BranchId,
            ServiceId: request.ServiceId,
            StaffId: request.StaffId,
            CustomerStatus: request.CustomerStatus,
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
            ReservationDate = a.ReservationDate,
            Status = a.Status,
            EstimatedDuration = a.EstimatedDuration,
            ServiceName = a.Service?.Name ?? string.Empty,
            StaffName = $"", // staff not implemented yet
            Cost = $"{a.Service?.Cost.Amount}{a.Service?.Cost.Currency}"
        }).ToList();
        return Result.Success(new PaginatedModel<AppointmentsResponse>(response, request.Page, request.PageSize, response.Count));
    }
}
