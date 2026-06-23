using Microsoft.EntityFrameworkCore;
using Viora.Domain.ChatSessions;

namespace Viora.Infrastructure.Repositories;

public sealed class ChatSessionRepository : IChatSessionRepository
{
    private readonly ApplicationDbContext _db;

    public ChatSessionRepository(ApplicationDbContext db) => _db = db;

    public async Task CreateAsync(ChatSession session, CancellationToken ct = default)
    {
        _db.Set<ChatSession>().Add(session);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateHistoryAsync(Guid sessionId, string historyJson, DateTime lastActiveAt, CancellationToken ct = default)
    {
        await _db.Set<ChatSession>()
            .Where(s => s.Id == sessionId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.HistoryJson, historyJson)
                .SetProperty(x => x.LastActiveAt, lastActiveAt),
                ct);
    }

    public async Task<ChatSession?> GetByIdAsync(Guid sessionId, CancellationToken ct = default)
        => await _db.Set<ChatSession>().AsNoTracking().FirstOrDefaultAsync(s => s.Id == sessionId, ct);

    public async Task<IReadOnlyList<ChatSession>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        // Exclude HistoryJson from list queries — it can be large
        return await _db.Set<ChatSession>()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.LastActiveAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new ChatSession
            {
                Id = s.Id,
                UserId = s.UserId,
                CreatedAt = s.CreatedAt,
                LastActiveAt = s.LastActiveAt,
                Title = s.Title,
                HistoryJson = string.Empty,
            })
            .ToListAsync(ct);
    }
}