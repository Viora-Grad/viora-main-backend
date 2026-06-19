using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Api.Extensions;
using Viora.Application.Customers.CreateCustomerProfile;
using Viora.Application.Customers.UpdateCustomerPicture;

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
    public IActionResult AuthorizationTester()
    {
        return Ok("You are authorized to access this endpoint.");
    }
}
