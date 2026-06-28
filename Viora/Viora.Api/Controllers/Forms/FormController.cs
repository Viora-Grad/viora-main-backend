using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Viora.Api.Extensions;
using Viora.Application.Abstractions.Media;
using Viora.Application.Forms.CreateForm;
using Viora.Application.Forms.DeleteForm;
using Viora.Application.Forms.GetForm;
using Viora.Application.Forms.GetFormSubmissionByAppointment;
using Viora.Application.Forms.GetServiceForm;
using Viora.Application.Forms.SubmitFormAnswer;
using Viora.Application.Forms.UpdateForm;

namespace Viora.Api.Controllers.Forms;

[ApiController]
public class FormController : ControllerBase
{
    private readonly ISender _sender;

    public FormController(ISender sender)
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
    [Route("api/service/form/{formId}")]

    public async Task<IActionResult> GetForm(Guid formId, CancellationToken cancellationToken)
    {
        var query = new GetFormByIdQuery(formId);
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


    [HttpPost]
    [Route("api/appontment/{appointmentId}/form-submission")]
    [Authorize]
    [Consumes("multipart/form-data")]

    public async Task<IActionResult> submitForm(
        [FromBody] FormSubmissionRequest request,
        [FromForm] IFormFileCollection files,
        [FromQuery] Guid appointmentId,
        [FromServices] IStorageSettings storageSetting,
        CancellationToken cancellationToke
        )
    {

        List<MediaRequest> mediaRequests = new List<MediaRequest>();
        if (files.Any())
        {
            try
            {
                foreach (var file in files)
                {
                    mediaRequests.Add(file.ContentType == "form-submission/pdf"
                         ? MediaRequest.CreateDocument(file.FileName, file.ContentType, file.Length, file.OpenReadStream(), storageSetting.MaxFileSizeBytes)
                         : MediaRequest.CreateImage(file.FileName, file.ContentType, file.Length, file.OpenReadStream(), storageSetting.MaxFileSizeBytes)

                        );
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        var command = new SubmitFormAnswerCommand(
                appointmentId,
                request.FormId,
                request.submission,
                mediaRequests
                );

        var result = await _sender.Send(command);

        return result.ToActionResult();
    }



    [HttpGet]
    [Authorize]
    [Route("api/form/{formId}/submission/{appointmentId}")]
    public async Task<IActionResult> GetAppountmentSubmission(Guid formId, Guid appointmentId, CancellationToken cancellationToke)
    {
        var query = new GetFormSubmissionByAppointmentQuery(appointmentId, formId);
        var result = await _sender.Send(query);
        return result.ToActionResult();
    }

    [HttpGet]
    [Authorize]
    [Route("api/form/submission/{FormSubmissionId}")]

    public async Task<IActionResult> GetSubmission(Guid FormSubmissionId, CancellationToken cancellationToke)
    {
        var query = new GetFormByIdQuery(FormSubmissionId);
        var result = await _sender.Send(query);
        return result.ToActionResult();
    }
}
