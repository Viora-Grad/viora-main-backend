using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Exceptions;

namespace Viora.Infrastructure.Authentication;

internal class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid UserId =>
        httpContextAccessor
        .HttpContext?
        .User
        .GetUserId() ??
        throw new InvalidOperationException("User is not authenticated or does not have a 'sub' claim.");

    public string UserType =>
        httpContextAccessor
        .HttpContext?
        .User
        .FindFirstValue("type") ??
        throw new InvalidOperationException("User is not authenticated or does not have a 'type' claim.");

    public Guid? OrganizationId =>
        httpContextAccessor
        .HttpContext?
        .User
        .FindFirstValue("organizationId") is string orgIdValue && Guid.TryParse(orgIdValue, out var orgId) ?
            orgId : null;
}
internal static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userId, out var guid) ?
            guid : throw new NotFoundException("User Not Found");
    }
}
