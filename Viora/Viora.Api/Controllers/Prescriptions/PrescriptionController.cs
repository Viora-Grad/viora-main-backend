using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Api.Extensions;
using Viora.Application.Abstractions.Media;
using Viora.Application.Prescriptions.CreatePrescription;
using Viora.Application.Prescriptions.CreatePrescriptionTemplate;
using Viora.Application.Prescriptions.GetOrganizationPrescroptionTemplate;
using Viora.Application.Prescriptions.GetPrescriptionByAppointment;
using Viora.Application.Prescriptions.GetPrescriptionById;
using Viora.Application.Prescriptions.GetPrescriptionFile;
using Viora.Application.Prescriptions.GetTemplateById;

namespace Viora.Api.Controllers.Prescriptions;

[ApiController]
public class PrescriptionController : ControllerBase
{
    private readonly ISender _sender;

    public PrescriptionController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [Route("api/prescription/create")]
    [Authorize(Policy = "prescription:write")]
    public async Task<IActionResult> CreatePrescription(PrescriptionRequest request, CancellationToken cancellationToken)
    {
        var command = new CreatePrescriptionCommand(request.AppointmentId, request.Items);
        var result = await _sender.Send(command);
        return result.ToActionResult();
    }



    [HttpPost]
    [Authorize(Policy = "prescriptionTemplate:write")]
    [Route("api/prescription-template/create")]
    public async Task<IActionResult> CreatePrescriptionTemplate(
        [FromForm] PrescriptionTemplateRequest request,
        [FromServices] IStorageSettings storageSettings,
        CancellationToken cancellationToken
        )
    {
        MediaRequest media;
        try
        {
            media = request.File.ContentType == "application/pdf"
                ? MediaRequest.CreateDocument(request.File.FileName, request.File.ContentType, request.File.Length, request.File.OpenReadStream(), storageSettings.MaxFileSizeBytes)
                : MediaRequest.CreateImage(request.File.FileName, request.File.ContentType, request.File.Length, request.File.OpenReadStream(), storageSettings.MaxFileSizeBytes);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }


        var command = new CreatePrescriptionTemplateCommand(
            request.OrganizationId,
            request.Name,
            media,
            request.TopMargin,
            request.RightMargin,
            request.LeftMargin,
            request.BottomMargin
            );
        var result = await _sender.Send(command);
        return result.ToActionResult();
    }

    [HttpGet]
    [Authorize(Policy = "organizationPrescriptionTemplate:read")]
    [Route("api/{organizationId}/prescription-template/")]
    public async Task<IActionResult> GetOrganizationPrescription(Guid organizationId, CancellationToken cancellationToken)
    {
        var query = new GetOrganizaionPrescriptionTamplateQuery(organizationId);
        var result = await _sender.Send(query);
        return result.ToActionResult();
    }

    [HttpGet]
    [Authorize(Policy = "prescriptionTemplate:read")]
    [Route("api/prescription-template/{id}")]
    public async Task<IActionResult> GetprescriptionTemplate(Guid Id, CancellationToken cancellationToken)
    {
        var query = new GetPrescriptionTemplateByIdQuery(Id);
        var result = await _sender.Send(query);
        return result.ToActionResult();
    }


    [HttpGet]
    [Authorize(Policy = "prescription:read")]
    [Route("api/prescription/appointment/{appointmentId}")]

    public async Task<IActionResult> GetAppointmentPrescription(Guid appointmentId, CancellationToken cancellationToken)
    {
        var query = new GetPrescriptionByAppointmentQuery(appointmentId);
        var result = await _sender.Send(query);
        return result.ToActionResult();

    }

    [HttpGet]
    [Authorize(Policy = "prescription:read")]
    [Route("api/prescription/{id}")]

    public async Task<IActionResult> GetPrescriptionById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPrescriptionByIdQuery(id);
        var result = await _sender.Send(query);
        return result.ToActionResult();

    }


    [HttpGet]
    [Authorize(Policy = "prescription:read")]
    [Route("api/prescription-template/{templateId}/File")]

    public async Task<IActionResult> GetTemplateFile(Guid templateId, CancellationToken cancellationToken)
    {
        var query = new GetPrescriptionTemplateFileQuery(templateId);
        var result = await _sender.Send(query);
        if (result.IsFailure)
            return result.ToActionResult();

        var file = result.Value;
        return File(file.Content, file.ContentType, file.FileName);

    }
}
