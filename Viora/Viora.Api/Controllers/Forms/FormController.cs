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
using Viora.Application.Forms.GetFormSubmissionById;
using Viora.Application.Forms.GetFormSubmissionFile;
using Viora.Application.Forms.GetServiceForm;
using Viora.Application.Forms.SubmitFormAnswer;
using Viora.Application.Forms.UpdateForm;
using Viora.Application.Forms.UploadFormSubmissionFile;

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
    [Authorize(Policy = "form:read")]
    public async Task<IActionResult> GetServiceForm(Guid serviceId, CancellationToken cancellationToken)
    {
        var query = new GetServiceFormQuery(serviceId);
        var result = await _sender.Send(query);
        return result.ToActionResult();
    }

    [HttpGet]
    [Route("api/service/form/{formId}")]
    [Authorize(Policy = "form:read")]
    public async Task<IActionResult> GetForm(Guid formId, CancellationToken cancellationToken)
    {
        var query = new GetFormByIdQuery(formId);
        var result = await _sender.Send(query);
        return result.ToActionResult();
    }

    [HttpPost]
    [Route("api/service/form/create")]
    [Authorize(Policy = "form:write")]
    public async Task<IActionResult> CreateForm([FromBody] CreateFormRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateFormCommand(request.ServiceId, request.StaffId, request.name, request.fields);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut]
    [Route("api/service/form/update/{formId}")]
    [Authorize(Policy = "form:write")]
    public async Task<IActionResult> UpdateForm(Guid formId, JsonDocument fields, CancellationToken cancellationToken)
    {
        var command = new UpdateFormCommand(formId, fields);
        var result = await _sender.Send(command);
        return result.ToActionResult();
    }

    [HttpDelete]
    [Route("api/service/form/delete/{formId}")]
    [Authorize(Policy = "form:write")]
    public async Task<IActionResult> DeleteForm(Guid formId, CancellationToken cancellationToken)
    {
        var command = new DeleteFormCommand(formId);
        var result = await _sender.Send(command);
        return result.ToActionResult();
    }


    [HttpPost]
    [Route("api/appontment/{appointmentId}/form-submission")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> submitForm(
        [FromBody] FormSubmissionRequest request,
         Guid appointmentId,
        CancellationToken cancellationToke
        )
    {

        var command = new SubmitFormAnswerCommand(
                appointmentId,
                request.FormId,
                request.submission
                );

        var result = await _sender.Send(command);

        return result.ToActionResult();
    }



    [HttpGet]
    [Authorize(policy: "formSubmission:read")]
    [Route("api/form/{formId}/submission/{appointmentId}")]
    public async Task<IActionResult> GetAppountmentSubmission(Guid formId, Guid appointmentId, CancellationToken cancellationToke)
    {
        var query = new GetFormSubmissionByAppointmentQuery(appointmentId, formId);
        var result = await _sender.Send(query);
        return result.ToActionResult();
    }

    [HttpGet]
    [Authorize(policy: "formSubmission:read")]
    [Route("api/form/submission/{FormSubmissionId}")]

    public async Task<IActionResult> GetSubmission(Guid FormSubmissionId, CancellationToken cancellationToke)
    {
        var query = new GetFormSubmissionByIdQuery(FormSubmissionId);
        var result = await _sender.Send(query);
        return result.ToActionResult();
    }


    [HttpPost]
    [Route("api/form/submission/file/{FormSubmissionId}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> UploadFormSubmissionFile(
        [FromForm] IFormFile File,
        Guid FormSubmissionId,
        IStorageSettings storageSettings,
        CancellationToken cancellationToken
        )
    {
        MediaRequest media;
        try
        {
            media = File.ContentType == "application/pdf"
                ? MediaRequest.CreateDocument(File.FileName, File.ContentType, File.Length, File.OpenReadStream(), storageSettings.MaxFileSizeBytes)
                : MediaRequest.CreateImage(File.FileName, File.ContentType, File.Length, File.OpenReadStream(), storageSettings.MaxFileSizeBytes);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        var command = new UploadFormSubmissionFileCommand(FormSubmissionId, media);
        var result = await _sender.Send(command);
        return result.ToActionResult();
    }


    [HttpGet]
    [Route("api/form/submission/file/{FormSubmission}/{FileId}")]
    [Authorize(policy: "formSubmission:read")]
    public async Task<IActionResult> GetFormSubmissionFileById(Guid FormSubmission, Guid FileId, CancellationToken cancellationToken)
    {
        var query = new GetFormSubmissionFileQuery(FormSubmission, FileId);
        var result = await _sender.Send(query);
        if (result.IsFailure)
            return result.ToActionResult();

        var file = result.Value;
        return File(file.Content, file.ContentType, file.FileName);

    }

}
