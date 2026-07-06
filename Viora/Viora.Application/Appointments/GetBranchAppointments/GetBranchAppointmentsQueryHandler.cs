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
            PaymentId: request.PaymentId,
            CustomerStatus: request.CustomerStatus,
            PaymentMethods: request.PaymentMethods,
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
            CustomerId = a.CustomerId,
            ServiceId = a.ServiceId,
            StaffId = a.StaffId,
            BranchId = a.BranchId,
            PaymentMethod = a.PayMethod.ToString(),
            PaymentId = a.PaymentId,
            ReservationDate = a.ReservationDate,
            Status = a.Status.ToString(),
            CustomerName = $"{a.Customer?.PersonalInfo.FirstName ?? string.Empty} {a.Customer?.PersonalInfo.LastName ?? string.Empty}".Trim(),
            EstimatedDurationMinutes = a.EstimatedDurationMinutes,
            ServiceName = a.Service?.Name ?? string.Empty,
            StaffName = $"{a.Staff?.FirstName.Value ?? string.Empty} {a.Staff?.LastName.Value ?? string.Empty}".Trim(),
            Cost = $"{a.Service?.Cost.ToString()}"
        }).ToList();
        return Result.Success(new PaginatedModel<AppointmentsResponse>(response, request.Page, request.PageSize, response.Count));
    }
}
