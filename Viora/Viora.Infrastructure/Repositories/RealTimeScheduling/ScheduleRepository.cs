using Microsoft.EntityFrameworkCore;
using Viora.Domain.RealTimeScheduling;

namespace Viora.Infrastructure.Repositories.RealTimeScheduling;

internal class ScheduleRepository : Repository<Schedule>, IScheduleRepository
{
    public ScheduleRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
    public async Task<Schedule?> getByBranchIdAndDayAsync(Guid branchId, DayOfWeek dayOfWeek, CancellationToken cancellationToken)
    {
        return await DbContext.Set<Schedule>()
            .Include(x => x.Intervals)
            .FirstOrDefaultAsync(
            s => s.BranchId == branchId
            && s.DayOfWeek == dayOfWeek,
            cancellationToken);
    }

    public async Task<List<Schedule>> getByBranchIdAsync(Guid branchId, CancellationToken cancellationToken)
    {
        return await DbContext.Set<Schedule>()
            .Where(s => s.BranchId == branchId)
            .Include(s => s.Intervals)
            .ToListAsync(cancellationToken);
    }
}
