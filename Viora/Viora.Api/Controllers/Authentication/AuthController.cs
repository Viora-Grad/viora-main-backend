using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Application.Authentication.ConsumeRefreshToken;
using Viora.Application.Users.GetLoggedInUser;
using Viora.Application.Users.LocalLoginUser;
using Viora.Application.Users.OAuthValidateToken;
using Viora.Application.Users.RegisterUser;

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
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        else
        {
            return Unauthorized(result.Error);
        }
    }
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (!Enum.IsDefined(request.Gender))
        {
            return BadRequest("Invalid gender value.");
        }
        var command = new RegisterUserCommand(
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.Gender,
            request.Email,
            request.Password);

        var result = await _sender.Send(command, cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        else
        {
            return BadRequest(result.Error);
        }
    }
    [HttpPost]
    [Route("refresh")]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var command = new ConsumeRefreshTokenCommand(request.RefreshToken);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        else
        {
            return BadRequest(result.Error);
        }
    }
    [HttpPost]
    [Route("oauth/{provider=google}/login")]
    public async Task<IActionResult> OAuthLogin(string provider, OAuthLoginRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    [HttpPost]
    [Route("oauth/{provider=google}/register")]
    public async Task<IActionResult> OAuthRegister(string provider, OAuthRegisterRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    [HttpPost]
    [Route("oauth/{provider=google}/validate")]
    public async Task<IActionResult> OAuthValidate(string provider, OAuthValidateRequest request, CancellationToken cancellationToken = default)
    {
        var command = new OAuthValidateTokenCommand(provider, request.Token);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        else
        {
            return BadRequest(result.Error);
        }
    }
    [HttpGet]
    [Route("me")]
    [Authorize(Policy = "users:read")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        var query = new GetLoggedInUserQuery();
        var result = await _sender.Send(query, cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        else
        {
            return Unauthorized(result.Error);
        }
    }
}