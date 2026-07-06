using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Api.Extensions;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Appointments.CancelAppointment;
using Viora.Application.Appointments.CheckInAppointment;
using Viora.Application.Appointments.CompleteAppointment;
using Viora.Application.Appointments.CreateAppointment;
using Viora.Application.Appointments.DelayAppointment;
using Viora.Application.Appointments.GetAppointment;
using Viora.Application.Appointments.GetBranchAppointments;
using Viora.Application.Appointments.GetCustomerAllAppointments;
using Viora.Application.Appointments.GetDoctorAppointments;
using Viora.Application.Appointments.NoShowAppointment;

namespace Viora.Api.Controllers.Appointments;

[Route("api/appointments")]
[Authorize]
[ApiController]
public class AppointmentsController(
    ISender sender,
    IUserContext userContext
    ) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "appointments:create,appointments:write")]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateAppointmentCommand(
            StaffId: request.StaffId,
            ServiceId: request.ServiceId,
            PaymentId: null,
            ReservationDate: request.ReservationDate,
            PaymentMethod: request.PaymentMethod,
            Status: request.Status,
            CreatedBy: request.CreatedBy,
            RequestPlatform: request.RequestPlatform
        );
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
    [HttpPatch("{id:guid}/check-in")]
    [Authorize(Policy = "appointments:write")] // might change the policy later to be more specific to the staff role
    public async Task<IActionResult> CheckInAppointment([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new CheckInAppointmentCommand(AppointmentId: id);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
    [HttpPatch("{id:guid}/complete")]
    [Authorize(Policy = "appointments:write")]
    public async Task<IActionResult> CompleteAppointment([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new CompleteAppointmentCommand(AppointmentId: id);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
    [HttpPatch("{id:guid}/cancel")]
    [Authorize(Policy = "appointments:cancel,appointments:write")]
    public async Task<IActionResult> CancelAppointment([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new CancelAppointmentCommand(AppointmentId: id);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
    [HttpPatch("{id:guid}/delay")]
    [Authorize(Policy = "appointments:write")]
    public async Task<IActionResult> DelayAppointment([FromRoute] Guid id, [FromBody] int delayDurationInMinutes, CancellationToken cancellationToken)
    {
        var delay = TimeSpan.FromMinutes(delayDurationInMinutes);
        var command = new DelayAppointmentCommand(AppointmentId: id, DelayDuration: delay);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetAppointment([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetAppointmentQuery(AppointmentId: id);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }
    [HttpGet("/branches/{branchId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetAppointmentsByBranch([FromRoute] Guid branchId, [FromQuery] GetAppointmentsQueryParams queryParams, CancellationToken cancellationToken)
    {
        var query = new GetBranchAppointmentsQuery(
            BranchId: branchId,
            CustomerId: queryParams.CustomerId,
            ServiceId: queryParams.ServiceId,
            StaffId: queryParams.StaffId,
            CustomerStatus: queryParams.CustomerStatus,
            IncludeCustomerObject: queryParams.IncludeCustomerObject,
            IncludeStaffObject: queryParams.IncludeStaffObject,
            IncludeServiceObject: queryParams.IncludeServiceObject,
            IncludeBranchObject: queryParams.IncludeBranchObject,
            ReservationDate: queryParams.ReservationDate,
            FromDate: queryParams.FromDate,
            ToDate: queryParams.ToDate,
            Page: queryParams.Page,
            PageSize: queryParams.PageSize
            );
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }
    [HttpGet("/doctors/{doctorId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetAppointmentsByDoctor([FromRoute] Guid doctorId, [FromQuery] GetAppointmentsQueryParams queryParams, CancellationToken cancellationToken)
    {

        var query = new GetDoctorAppointmentsQuery(
            StaffId: doctorId,
            CustomerId: queryParams.CustomerId,
            ServiceId: queryParams.ServiceId,
            BranchId: queryParams.BranchId,
            CustomerStatus: queryParams.CustomerStatus,
            IncludeCustomerObject: queryParams.IncludeCustomerObject,
            IncludeStaffObject: queryParams.IncludeStaffObject,
            IncludeServiceObject: queryParams.IncludeServiceObject,
            IncludeBranchObject: queryParams.IncludeBranchObject,
            ReservationDate: queryParams.ReservationDate,
            FromDate: queryParams.FromDate,
            ToDate: queryParams.ToDate,
            Page: queryParams.Page,
            PageSize: queryParams.PageSize
            );
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }
    [HttpGet("/customers/{customerId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetAppointmentsByCustomer([FromRoute] Guid customerId, [FromQuery] GetAppointmentsQueryParams queryParams, CancellationToken cancellationToken)
    {
        if (userContext.UserId != customerId)
        {
            return Forbid();
        }
        var query = new GetCustomerAllAppointmentsQuery(
            CustomerId: customerId,
            BranchId: queryParams.BranchId,
            ServiceId: queryParams.ServiceId,
            StaffId: queryParams.StaffId,
            CustomerStatus: queryParams.CustomerStatus,
            IncludeCustomerObject: queryParams.IncludeCustomerObject,
            IncludeStaffObject: queryParams.IncludeStaffObject,
            IncludeServiceObject: queryParams.IncludeServiceObject,
            IncludeBranchObject: queryParams.IncludeBranchObject,
            ReservationDate: queryParams.ReservationDate,
            FromDate: queryParams.FromDate,
            ToDate: queryParams.ToDate,
            Page: queryParams.Page,
            PageSize: queryParams.PageSize
            );
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }
    [HttpPatch("{appointmentId:guid}/no-show")]
    [Authorize(Policy = "appointments:write")]
    public async Task<IActionResult> MarkAsNoShow([FromRoute] Guid appointmentId, CancellationToken cancellationToken)
    {
        var command = new NoShowAppointmentCommand(AppointmentId: appointmentId);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
}
