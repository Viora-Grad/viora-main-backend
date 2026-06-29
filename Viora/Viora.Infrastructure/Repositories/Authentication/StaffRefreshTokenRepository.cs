using Microsoft.EntityFrameworkCore;
using Viora.Infrastructure.Authentication;

namespace Viora.Infrastructure.Repositories.Authentication;

public class StaffRefreshTokenRepository(ApplicationDbContext dbContext)
{
    public void Add(StaffRefreshToken token)
    {
        dbContext.Set<StaffRefreshToken>().Add(token);
    }
    public async Task<StaffRefreshToken?> GetActiveStaffTokenByStaffIdAsync(Guid staffId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<StaffRefreshToken>()
            .Where(rt => rt.StaffId == staffId && !rt.IsRevoked)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
