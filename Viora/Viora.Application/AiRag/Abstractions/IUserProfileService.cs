using Viora.Domain.AiRag;

namespace Viora.Application.AiRag.Abstractions;

public interface IUserProfileService
{
    Task<UserContext?> GetUserContextAsync(Guid userId, CancellationToken ct = default);
}
