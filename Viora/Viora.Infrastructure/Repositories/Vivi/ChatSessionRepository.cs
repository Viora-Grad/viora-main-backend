using Microsoft.EntityFrameworkCore;
using Viora.Domain.Vivi.ChatSessions;
using Viora.Domain.Vivi.Shared.Internals;

namespace Viora.Infrastructure.Repositories.Vivi;

internal sealed class ChatSessionRepository(ApplicationDbContext dbContext) : IChatSessionRepository
{

    public async Task<IEnumerable<ChatSessionSummary>> GetSessionsByUserIdAsync(Guid userId, Persona persona, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<ChatSession>()
            .AsNoTracking()
            .Where(s => s.ChatterId == userId && s.Persona == persona)
            .OrderByDescending(s => s.LatestActivityUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new ChatSessionSummary(s.Id, s.Name, s.LatestActivityUtc))
            .ToListAsync(cancellationToken);
    }
}
