using Microsoft.EntityFrameworkCore;
using Viora.Domain.Notifications;

namespace Viora.Infrastructure.Repositories.Notifications;

internal class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<List<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Notification>()
            .Where(n => n.RecipientId == userId).ToListAsync(cancellationToken);
    }
}
