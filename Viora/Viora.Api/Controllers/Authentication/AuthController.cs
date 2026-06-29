using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Viora.Api.Extensions;
using Viora.Application.Authentication.ChangePassword;
using Viora.Application.Authentication.ConfirmForgetPassword;
using Viora.Application.Authentication.ConsumeRefreshToken;
using Viora.Application.Authentication.CreateStaffRole;
using Viora.Application.Authentication.ForgetPassword;
using Viora.Application.Authentication.GetOrganizationRoles;
using Viora.Application.Authentication.ValidateEmail;
using Viora.Application.Authentication.ValidateUsername;
using Viora.Application.Staffs.RegisterStaff;
using Viora.Application.Users.GetLoggedInUser;
using Viora.Application.Users.LocalLoginUser;
using Viora.Application.Users.OAuthLoginUser;
using Viora.Application.Users.OAuthRegisterUser;
using Viora.Application.Users.OAuthValidateToken;
using Viora.Application.Users.RegisterUser;
using Viora.Domain.Users.Identity;

namespace Viora.Api.Controllers.Authentication;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;
    public AuthController(ISender sender)
    {
        _sender = sender;
    }
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var command = new LocalLoginUserCommand(request.Email, request.Password);

        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var command = new RegisterUserCommand(
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.Gender,
            request.Email,
            request.Password);

        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
    [HttpPost]
    [Route("refresh")]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var command = new ConsumeRefreshTokenCommand(request.RefreshToken);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
    [HttpPost]
    [Route("oauth/{provider=google}/login")]
    public async Task<IActionResult> OAuthLogin(string provider, OAuthLoginRequest request, CancellationToken cancellationToken = default)
    {
        var command = new OAuthLoginUserCommand(provider, request.Token, request.Code, request.RedirectUri);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
    [HttpPost]
    [Route("oauth/{provider=google}/register")]
    public async Task<IActionResult> OAuthRegister(string provider, OAuthRegisterRequest request, CancellationToken cancellationToken = default)
    {
        var OAuthRegisterCommand = new OAuthRegisterUserCommand(
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.Gender,
            request.Email,
            provider,
            request.ProviderKey);
        var result = await _sender.Send(OAuthRegisterCommand, cancellationToken);
        return result.ToActionResult();
    }
    [HttpPost]
    [Route("oauth/{provider=google}/validate")]
    public async Task<IActionResult> OAuthValidate(string provider, OAuthValidateRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.IsValid)
        {
            return BadRequest("Invalid request parameters.");
        }
        if (request.IsToken)
        {
            var token = request.Token;
            var command = new OAuthValidateTokenCommand(provider, token, null, null);
            var result = await _sender.Send(command, cancellationToken);
            return result.ToActionResult();
        }
        else if (request.IsCode)
        {
            var code = request.Code;
            var redirectUri = request.RedirectUri;
            var command = new OAuthValidateTokenCommand(provider, null, code, redirectUri);
            var result = await _sender.Send(command, cancellationToken);
            return result.ToActionResult();
        }
        else
        {
            return BadRequest("Could not validate the request.");
        }
    }

    [HttpGet]
    [Route("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        var query = new GetLoggedInUserQuery();
        var result = await _sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }
    [HttpPost]
    [Route("validate/email")]
    public async Task<IActionResult> ValidateEmail(ValidateEmailRequest request, CancellationToken cancellationToken = default)
    {
        var command = new ValidateEmailCommand(request.Email);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    [Route("forget-password")]
    public async Task<IActionResult> ForgetPassword([FromBody] string email, CancellationToken cancellationToken = default)
    {
        var command = new ForgetPasswordCommand(email);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    [Route("confirm-forget-password")]
    public async Task<IActionResult> ConfirmForgetPassword(ConfirmForgetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        IPAddress? remoteIp = HttpContext.Connection.RemoteIpAddress;

        var ipv4Address = remoteIp != null
            ? remoteIp.MapToIPv4().ToString()
            : "Unknown";

        var command = new ConfirmForgetPasswordCommand(request.Email, request.Otp, request.NewPassword, ipv4Address);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    [Route("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        IPAddress? remoteIp = HttpContext.Connection.RemoteIpAddress;

        var ipv4Address = remoteIp != null
            ? remoteIp.MapToIPv4().ToString()
            : "Unknown";

        var command = new ChangePasswordCommand(request.CurrentPassword, request.NewPassword, ipv4Address);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
    [HttpPost("organization/{organizationId:guid}/validate-username")]
    public async Task<IActionResult> ValidateUsername(Guid organizationId, ValidateUsernameRequest request, CancellationToken cancellationToken = default)
    {
        var command = new ValidateUsernameCommand(organizationId, request.Value);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
    [HttpPost("organization/{organizationId:guid}/staff/register")]
    public async Task<IActionResult> RegisterStaff(Guid organizationId, RegisterStaffRequest request, CancellationToken cancellationToken = default)
    {
        var command = new RegisterStaffCommand(
            organizationId,
            request.Token,
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.Gender,
            request.PhoneNumber,
            request.Username,
            request.Password);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
    [HttpGet("permissions")]
    [Authorize(Policy = "permissions:read")]
    public IActionResult GetPermissions(CancellationToken cancellationToken = default)
    {
        return Ok(Permission.All);
    }
    [HttpGet("organization/{organizationId:guid}/roles")]
    [Authorize(Policy = "roles:read")]
    public async Task<IActionResult> GetRoles(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var query = new GetOrganizationRolesQuery(organizationId);
        var result = await _sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }
    [HttpPost("organizations/{organizationId:guid}/role")]
    [Authorize(Policy = "roles:write")]
    public async Task<IActionResult> CreateStaffRole(Guid organizationId, CreateStaffRoleRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateStaffRoleCommand(
            organizationId,
            request.RoleName,
            request.RoleDescription,
            [.. request.PermissionsIds]);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
}