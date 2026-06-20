using Microsoft.EntityFrameworkCore;
using Viora.Domain.RealTimeScheduling;

namespace Viora.Infrastructure.Repositories.RealTimeScheduling;

internal class ShiftRepository : Repository<Shift>, IShiftRepository
{
    public ShiftRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Shift?> GetActiveShiftAsync(Guid ScheduleId, Guid Staff, TimeOnly time, CancellationToken cancellationToken)
    {
        return await DbContext.Set<Shift>().FirstOrDefaultAsync(s => s.ScheduleId == ScheduleId &&
            s.StaffId == Staff &&
            s.StartTime <= time &&
            s.EndTime >= time, cancellationToken);
    }
}
