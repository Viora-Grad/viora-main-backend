using Microsoft.AspNetCore.Http;
using Viora.Application.Abstractions.Authentication;

namespace Viora.Infrastructure.Authentication;

internal class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid UserId => Guid.Parse(
        httpContextAccessor
        .HttpContext?.User?
        .Claims.FirstOrDefault(c => c.Type == "sub")?.Value ??
        throw new InvalidOperationException("User is not authenticated or does not have a 'sub' claim."));
}
