using MediatR;
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
    public async Task<IActionResult> CreateSchedule(CreateScheduleRequest request)
    {
        var command = new CreateScheduleCommand(request.BranchId, request.DayOfWeek);
        var result = await _sender.Send(command);
        return result.ToActionResult();
    }

    [HttpPost]
    [Route("api/schedule/shift/create")]
    public async Task<IActionResult> CreateShift(CreateShiftRequest request)
    {
        var command = new CreateShiftCommand(request.BranchId, request.StartTime, request.EndTime, request.DayOfWeek, request.StaffId);
        var result = await _sender.Send(command);
        return result.ToActionResult();
    }

    [HttpPost]
    [Route("api/schedule/cancel")]
    public async Task<IActionResult> CancelSchedule(CancelScheduleRequest request)
    {
        var command = new CancelScheduleCommand(request.ShiftId, request.BranchId, request.cancellationDate, request.Reason);
        var result = await _sender.Send(command);
        return result.ToActionResult();
    }

    [HttpGet]
    [Route("api/schedule/{branchId}")]
    public async Task<IActionResult> GetBranchSchedule(Guid branchId)
    {
        var query = new GetBranchScheduleQuery(branchId);
        var result = await _sender.Send(query);
        return result.ToActionResult();
    }


    [HttpGet]
    [Route("api/schedule/staff/{staffId}")]
    public async Task<IActionResult> GetStaffShifts(Guid staffId)
    {
        var query = new GetStaffShiftQuery(staffId);
        var result = await _sender.Send(query);
        return result.ToActionResult();
    }

    [HttpGet]
    [Route("api/schedule/")]
    public async Task<IActionResult> GetStaffShift(StaffShiftByDayRequest request)
    {
        var query = new GetStaffShiftByDayQuery(request.day, request.StaffId, request.ShiftId);
        var result = await _sender.Send(query);
        return result.ToActionResult();
    }


    [HttpDelete]
    [Route("api/schedule/shift/delete/{id}")]
    public async Task<IActionResult> DeleteShift(Guid id)
    {
        var command = new DeleteShiftCommand(id);
        var result = await _sender.Send(command);
        return result.ToActionResult();
    }
}
