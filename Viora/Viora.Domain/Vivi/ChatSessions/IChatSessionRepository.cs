using Viora.Domain.Vivi.Shared.Internals;

namespace Viora.Domain.Vivi.ChatSessions;

public interface IChatSessionRepository
{
    public Task<IEnumerable<ChatSessionSummary>> GetSessionsByUserIdAsync
        (Guid userId, Persona persona, int page, int pageSize, CancellationToken cancellationToken = default);

    public Task<int> GetCountSessionsByUserIdAsync(Guid userId, Persona persona, CancellationToken cancellationToken = default);
}

public sealed record ChatSessionSummary(Guid Id, string Name, DateTime LatestActivityUtc);
