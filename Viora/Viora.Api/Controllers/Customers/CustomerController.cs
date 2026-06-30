using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Api.Extensions;
using Viora.Application.Customers.CreateCustomerProfile;
using Viora.Application.Customers.CreateMedicalRecord;
using Viora.Application.Customers.GetMedicalRecord;
using Viora.Application.Customers.UpdateCustomerPicture;
using Viora.Application.Customers.UpdateMedicalRecord;

namespace Viora.Api.Controllers.Customers;

[Route("api/customer")]
[Authorize]
[ApiController]
public class CustomerController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Route("/create")]
    public async Task<IActionResult> CreateCustomerProfile(CreateCustomerProfileRequest request, CancellationToken cancellationToken = default)
    {
        var command = new CreateCustomerProfileCommand(
            request.UserName,
            request.PhoneNumbers,
            request.Emails
            );
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
    [HttpPut]
    [Route("/profilepicture")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateProfilePicture(IFormFile file, CancellationToken cancellationToken = default)
    {
        await using var stream = file.OpenReadStream();
        var command = new UpdateCustomerPictureCommand(stream, file.Name, file.ContentType, file.Length);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
    [HttpGet]
    [Route("/profile")]
    public async Task<IActionResult> GetCustomerProfile(CancellationToken cancellationToken = default)
    {
        // Implementation for fetching customer profile
        return Ok();
    }
    [HttpPost("medicalrecord")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CreateMedicalRecord(CreateMedicalRecordRequest request, CancellationToken cancellationToken = default)
    {
        var command = new CreateMedicalRecordCommand(
            request.Systolic,
            request.Diastolic,
            request.Weight,
            request.HeartRate,
            request.BloodGlucose,
            [.. request.Allergies]
        );
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
    [HttpPatch("medicalrecord")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> UpdateMedicalRecord(UpdateMedicalRecordRequest request, CancellationToken cancellationToken = default)
    {
        var command = new UpdateMedicalRecordCommand(
            request.Systolic,
            request.Diastolic,
            request.Weight,
            request.HeartRate,
            request.BloodGlucose,
            [.. request.Allergies]
        );
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
    [HttpGet("medicalrecord")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMedicalRecord(CancellationToken cancellationToken = default)
    {
        var query = new GetMedicalRecordQuery();
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }
}
