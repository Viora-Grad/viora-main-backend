using Viora.Domain.RealTimeScheduling;

namespace Viora.Infrastructure.Repositories.RealTimeScheduling;

internal class ScheduleCancellationRepository : Repository<ScheduleCancellations>, IScheduleCancellationRepository
{
    public ScheduleCancellationRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}
