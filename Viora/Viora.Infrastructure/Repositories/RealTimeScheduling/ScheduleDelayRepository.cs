using Viora.Domain.RealTimeScheduling;

namespace Viora.Infrastructure.Repositories.RealTimeScheduling;

internal class ScheduleDelayRepository : Repository<ScheduleDelay>, IScheduleDelayRepository
{
    public ScheduleDelayRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

}
