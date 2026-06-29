using Microsoft.EntityFrameworkCore;
using Viora.Domain.Staffs;

namespace Viora.Infrastructure.Repositories.Staffs;

internal class StaffTokenRepository : Repository<StaffToken>, IStaffTokenRepository
{
    public StaffTokenRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<StaffToken?> GetByTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await DbContext.Set<StaffToken>()
            .Include(st => st.Staff)
            .FirstOrDefaultAsync(st => st.TokenHash == token, cancellationToken);
    }
}
