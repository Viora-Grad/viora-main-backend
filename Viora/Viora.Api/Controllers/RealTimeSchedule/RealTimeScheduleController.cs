using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Api.Extensions;
using Viora.Application.RealTimeScheduling.CancelSchedule;
using Viora.Application.RealTimeScheduling.CreateRecurringSchedule;
using Viora.Application.RealTimeScheduling.CreateSchedule;
using Viora.Application.RealTimeScheduling.DeleteShift;
using Viora.Application.RealTimeScheduling.GetBranchSchedule;
using Viora.Application.RealTimeScheduling.GetStaffShiftByDay;
using Viora.Application.RealTimeScheduling.GetStaffShiftQuery;

namespace Viora.Api.Controllers.RealTimeSchedule;


[ApiController]
public class RealTimeScheduleController : ControllerBase
{
    private readonly ISender _sender;

    public RealTimeScheduleController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [Route("api/schedule/create")]
    [Authorize(Policy = "schedule:write")]
    public async Task<IActionResult> CreateSchedule(CreateScheduleRequest request)
    {
        var command = new CreateScheduleCommand(request.BranchId, request.DayOfWeek);
        var result = await _sender.Send(command);
        return result.ToActionResult();
    }

    [HttpPost]
    [Route("api/schedule/shift/create")]
    [Authorize(Policy = "shift:write")]
    public async Task<IActionResult> CreateShift(CreateShiftRequest request)
    {
        var command = new CreateShiftCommand(request.BranchId, request.StartTime, request.EndTime, request.DayOfWeek, request.StaffId);
        var result = await _sender.Send(command);
        return result.ToActionResult();
    }

    [HttpPost]
    [Route("api/schedule/cancel")]
    [Authorize(Policy = "schedule:write")]
    public async Task<IActionResult> CancelSchedule(CancelScheduleRequest request)
    {
        var command = new CancelScheduleCommand(request.ShiftId, request.BranchId, request.cancellationDate, request.Reason);
        var result = await _sender.Send(command);
        return result.ToActionResult();
    }

    [HttpGet]
    [Route("api/schedule/{branchId}")]
    [Authorize(Policy = "schedule:read")]
    public async Task<IActionResult> GetBranchSchedule(Guid branchId)
    {
        var query = new GetBranchScheduleQuery(branchId);
        var result = await _sender.Send(query);
        return result.ToActionResult();
    }


    [HttpGet]
    [Route("api/branch/{branchId}/schedule/staff/{staffId}")]
    [Authorize(Policy = "shift:read")]
    public async Task<IActionResult> GetStaffShifts(Guid staffId, Guid branchId)
    {
        var query = new GetStaffShiftQuery(staffId, branchId);
        var result = await _sender.Send(query);
        return result.ToActionResult();
    }

    [HttpGet]
    [Route("api/schedule/staff")]
    [Authorize(Policy = "shift:read")]
    public async Task<IActionResult> GetStaffShift([FromQuery] StaffShiftByDayRequest request)
    {
        var query = new GetStaffShiftByDayQuery(request.day, request.StaffId, request.ShiftId);
        var result = await _sender.Send(query);
        return result.ToActionResult();
    }


    [HttpDelete]
    [Route("api/schedule/shift/delete/{id}")]
    [Authorize(Policy = "shift:write")]
    public async Task<IActionResult> DeleteShift(Guid id)
    {
        var command = new DeleteShiftCommand(id);
        var result = await _sender.Send(command);
        return result.ToActionResult();
    }
}
