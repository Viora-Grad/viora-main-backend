using Viora.Domain.RealTimeScheduling;

namespace Viora.Infrastructure.Repositories.RealTimeScheduling;

internal class ShiftRepository : Repository<Shift>, IShiftRepository
{
    public ShiftRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public Task<Shift?> GetActiveShiftAsync(Guid ScheduleId, Guid Staff, TimeOnly time, CancellationToken cancellationToken)
    {

    }
}
