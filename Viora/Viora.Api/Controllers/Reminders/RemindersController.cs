using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Api.Extensions;
using Viora.Application.Reminders.CreateReminder;
using Viora.Application.Reminders.GetCustomerReminders;
using Viora.Application.Reminders.GetReminder;

namespace Viora.Api.Controllers.Reminders;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RemindersController(ISender sender) : ControllerBase
{
    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> GetReminder(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetReminderQuery(id);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }
    [HttpPost]
    [Authorize(Policy = "reminders:write")]
    public async Task<IActionResult> CreateReminder([FromBody] CreateReminderRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateReminderCommand(request.AppointmentId, request.Title, request.Body, request.ScheduledFor);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
    [HttpGet]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetRemindersForCustomer(CancellationToken cancellationToken)
    {
        var command = new GetCustomerRemindersQuery();
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }


}
