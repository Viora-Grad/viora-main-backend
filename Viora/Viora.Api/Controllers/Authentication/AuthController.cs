using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Api.Extensions;
using Viora.Application.Authentication.ConsumeRefreshToken;
using Viora.Application.Authentication.ValidateEmail;
using Viora.Application.Users.GetLoggedInUser;
using Viora.Application.Users.LocalLoginUser;
using Viora.Application.Users.OAuthLoginUser;
using Viora.Application.Users.OAuthRegisterUser;
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
    [Authorize(Policy = "users:read")]
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

}