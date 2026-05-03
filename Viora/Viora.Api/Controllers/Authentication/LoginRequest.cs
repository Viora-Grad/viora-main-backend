
namespace Viora.Api.Controllers.Authentication;

public sealed record LoginRequest(
    string Email,
    string Password
);

