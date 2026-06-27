using Viora.Domain.Staffs;

namespace Viora.Infrastructure.Repositories.Staffs;

internal class StaffRepository : Repository<Staff>, IStaffRepository
{
    public StaffRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}
