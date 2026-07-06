using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Api.Extensions;
using Viora.Application.Branches.AddBranch;
using Viora.Application.Branches.GetBranchDetails;
using Viora.Application.Branches.GetBranchGallery;
using Viora.Application.Branches.GetBranchGalleryImage;
using Viora.Application.Branches.LinkImageToBranch;
using Viora.Application.Branches.SearchBranches;
using Viora.Application.Branches.UnlinkImageFromBranch;
using Viora.Application.Branches.UpdateBranchStatus;
using Viora.Application.Branches.UpdatePhoneNumbers;
using Viora.Application.Branches.UpdateSchedule;
using Viora.Domain.Branches.Internals;

namespace Viora.Api.Controllers.Branches;

[Route("api/[controller]")]
[ApiController]
public class BranchesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> SearchBranches(
        Guid? branchId,
        Guid? organizationId,
        double? longitude,
        double? latitude,
        bool? isCurrentlyOpen,
        [FromQuery] IEnumerable<string>? servicesFilter = null,
        [FromQuery] IEnumerable<string>? orderBy = null,
        BranchStatus status = BranchStatus.Active,
        double? distanceWithinMeters = null,
        double minimumRating = 0.0,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchBranchesQuery(
            branchId,
            organizationId,
            longitude,
            latitude,
            isCurrentlyOpen,
            servicesFilter,
            orderBy,
            status,
            distanceWithinMeters,
            minimumRating,
            page,
            pageSize);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBranchDetails(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetBranchDetailsQuery(id);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{branchId:guid}/gallery")]
    public async Task<IActionResult> GetBranchGallery(Guid branchId, CancellationToken cancellationToken)
    {
        var query = new GetBranchGalleryQuery(branchId);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{branchId:guid}/gallery/{mediaId:guid}/file")]
    public async Task<IActionResult> GetBranchGalleryImage(Guid branchId, Guid mediaId, CancellationToken cancellationToken)
    {
        var query = new GetBranchGalleryImageQuery(branchId, mediaId);
        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        var file = result.Value;
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> AddBranch(AddBranchRequest request, CancellationToken cancellationToken)
    {
        var command = new AddBranchCommand(
            request.OrganizationId,
            request.AddressNumber,
            request.AddressStreet,
            request.AddressCity,
            request.AddressState,
            request.AddressCountryId,
            request.AddressPostalCode,
            request.Latitude,
            request.Longitude,
            request.ContactEmail,
            request.ServicesProvided,
            request.TimeZoneId);
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult(
            createdAtAction: nameof(GetBranchDetails),
            routeValueFactory: val => new { id = val });
    }

    [HttpPut("{branchId:guid}/phone-numbers")]
    [Authorize(Policy = "branch:write")]
    public async Task<IActionResult> UpdatePhoneNumbers(Guid branchId, UpdatePhoneNumbersRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdatePhoneNumbersCommand(branchId, request.PhoneNumbers);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{branchId:guid}/status")]
    [Authorize(Policy = "branch:write")]
    public async Task<IActionResult> UpdateBranchStatus(Guid branchId, UpdateBranchStatusRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateBranchStatusCommand(branchId, request.Status);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{branchId:guid}/schedule")]
    [Authorize(Policy = "branch:write")]
    public async Task<IActionResult> UpdateSchedule(Guid branchId, UpdateScheduleRequest request, CancellationToken cancellationToken)
    {
        var schedule = new List<BusinessHour>();
        foreach (var hour in request.Schedule)
        {
            var businessHour = BusinessHour.Create(hour.Day, hour.OpenTime, hour.CloseTime);
            if (businessHour.IsFailure)
                return businessHour.ToActionResult();

            schedule.Add(businessHour.Value);
        }

        var command = new UpdateScheduleCommand(branchId, schedule);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("{branchId:guid}/gallery/{mediaId:guid}")]
    [Authorize(Policy = "branch:write")]
    public async Task<IActionResult> LinkImageToBranch(Guid branchId, Guid mediaId, CancellationToken cancellationToken)
    {
        var command = new LinkImageToBranchCommand(branchId, mediaId);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpDelete("{branchId:guid}/gallery/{imageId:guid}")]
    [Authorize(Policy = "branch:write")]
    public async Task<IActionResult> UnlinkImageFromBranch(Guid branchId, Guid imageId, CancellationToken cancellationToken)
    {
        var command = new UnlinkImageFromBranchCommand(branchId, imageId);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
}
