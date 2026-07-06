using Microsoft.EntityFrameworkCore;
using Viora.Domain.Marketing;

namespace Viora.Infrastructure.Repositories.Marketing;

internal sealed class MarketingChatSessionRepository : Repository<MarketingChatSession>, IMarketingChatSessionRepository
{
    public MarketingChatSessionRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<MarketingChatSession?> GetByIdWithMessagesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<MarketingChatSession>()
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<MarketingChatSession>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<MarketingChatSession>()
            .AsNoTracking()
            .Where(s => s.OrganizationId == organizationId)
            .OrderByDescending(s => s.UpdatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
