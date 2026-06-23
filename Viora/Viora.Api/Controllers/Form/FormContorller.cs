using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Viora.Api.Extensions;
using Viora.Application.Forms.CreateForm;
using Viora.Application.Forms.DeleteForm;
using Viora.Application.Forms.GetForm;
using Viora.Application.Forms.GetServiceForm;
using Viora.Application.Forms.UpdateForm;

namespace Viora.Api.Controllers.Form;

[ApiController]
public class FormContorller : ControllerBase
{
    private readonly ISender _sender;

    public FormContorller(ISender sender)
    {
        _sender = sender;
    }


    [HttpGet]
    [Route("api/service/{serviceId}/form")]
    public async Task<IActionResult> GetServiceForm(Guid serviceId, CancellationToken cancellationToken)
    {
        var query = new GetServiceFormQuery(serviceId);
        var result = await _sender.Send(query);
        return result.ToActionResult();
    }

    [HttpGet]
    [Route("api/service/form/{fprmId}")]

    public async Task<IActionResult> GetForm(Guid serviceId, CancellationToken cancellationToken)
    {
        var query = new GetFormByIdQuery(serviceId);
        var result = await _sender.Send(query);
        return result.ToActionResult();
    }

    [HttpPost]
    [Route("api/service/form/create")]
    public async Task<IActionResult> CreateForm(CreateFormRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateFormCommand(request.ServiceId, request.StaffId, request.name, request.fields);
        var result = await _sender.Send(command);
        return result.ToActionResult();
    }

    [HttpPut]
    [Route("api/service/form/update/{formId}")]
    public async Task<IActionResult> UpdateForm(Guid formId, JsonDocument fields, CancellationToken cancellationToken)
    {
        var command = new UpdateFormCommand(formId, fields);
        var result = await _sender.Send(command);
        return result.ToActionResult();
    }

    [HttpDelete]
    [Route("api/service/form/delete/{formId}")]
    public async Task<IActionResult> DeleteForm(Guid formId, CancellationToken cancellationToken)
    {
        var command = new DeleteFormCommand(formId);
        var result = await _sender.Send(command);
        return result.ToActionResult();
    }
}
